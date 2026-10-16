package com.companyname.prayertimeengine.wear;

import static androidx.wear.protolayout.DimensionBuilders.degrees;
import static androidx.wear.protolayout.DimensionBuilders.dp;
import static androidx.wear.protolayout.DimensionBuilders.expand;
import static androidx.wear.protolayout.DimensionBuilders.sp;

import androidx.wear.protolayout.ActionBuilders;
import androidx.wear.protolayout.ColorBuilders;
import androidx.wear.protolayout.DimensionBuilders;
import androidx.wear.protolayout.LayoutElementBuilders;
import androidx.wear.protolayout.ModifiersBuilders;
import androidx.wear.protolayout.TypeBuilders;
import androidx.wear.protolayout.expression.DynamicBuilders.DynamicFloat;
import androidx.wear.protolayout.expression.DynamicBuilders.DynamicInstant;
import androidx.wear.protolayout.expression.DynamicBuilders.DynamicInt32;
import androidx.wear.protolayout.expression.DynamicBuilders.DynamicString;

import java.time.Instant;

/**
 * Builds the "arc" tile layout: the bar of the phone notification bent along the edge of the round
 * display, with the countdown to its end in the middle.
 *
 * <p>Written in Java because the .NET binding of androidx.wear.protolayout leaves out every member
 * that takes a dimension (sizes, lengths, angles, paddings), so the layout cannot be built from C#.
 * All decisions about what to show are made in C#, this only turns them into layout elements.
 */
public final class PrayerTimeArcLayout {
    private static final int ACCENT = 0xFFEFB03E;
    private static final int TRACK = 0x40EFB03E;
    private static final int TEXT = 0xFFFFFFFF;
    private static final int TEXT_MUTED = 0xAAFFFFFF;

    /** The arc leaves the bottom of the display open, the times are written there. */
    private static final float ARC_START_DEGREES = -130f;
    private static final float ARC_SWEEP_DEGREES = 260f;
    private static final float SEGMENT_GAP_DEGREES = 3f;
    private static final float ARC_THICKNESS_DP = 7f;
    private static final float GAP_ARC_THICKNESS_DP = 4f;
    private static final float LABEL_INSET_DP = 12f;

    private PrayerTimeArcLayout() {
    }

    /**
     * @param segmentLengths  length of each section in percent, adding up to 100
     * @param labels          name of each transition between two sections
     * @param elapsedPercent  share which had passed when this layout becomes valid
     * @param isGap           the span between two prayer times, drawn dashed like on the phone
     */
    public static LayoutElementBuilders.LayoutElement create(
            String header,
            String countdownCaption,
            String footer,
            long startEpochSeconds,
            long endEpochSeconds,
            int[] segmentLengths,
            String[] labels,
            int elapsedPercent,
            boolean isGap,
            String packageName,
            String activityClassName) {
        LayoutElementBuilders.Box.Builder box = createRoot(packageName, activityClassName);

        float thickness = isGap ? GAP_ARC_THICKNESS_DP : ARC_THICKNESS_DP;
        float[] segmentDegrees = getSegmentDegrees(segmentLengths);

        if (isGap) {
            box.addContent(createDashedTrack(thickness));
        } else {
            box.addContent(createTrack(segmentDegrees, thickness));
        }

        DynamicFloat elapsedDegrees = getElapsedDegrees(startEpochSeconds, endEpochSeconds);
        float staticElapsedDegrees = ARC_SWEEP_DEGREES * elapsedPercent / 100f;

        float segmentStart = 0f;
        for (int i = 0; i < segmentDegrees.length; i++) {
            float segmentEnd = segmentStart + segmentDegrees[i];

            // sections which have passed are filled completely. The running one is filled by a
            // dynamic expression the watch evaluates every second. A layout never spans a sub
            // time, so all other sections stay as they are while it is shown.
            if (staticElapsedDegrees >= segmentEnd) {
                box.addContent(createFillArc(segmentStart, degrees(segmentDegrees[i]), thickness, null));
            } else if (staticElapsedDegrees >= segmentStart) {
                float staticLength = Math.max(0f, staticElapsedDegrees - segmentStart);
                DimensionBuilders.DegreesProp length = new DimensionBuilders.DegreesProp.Builder(staticLength)
                        .setDynamicValue(elapsedDegrees.minus(segmentStart))
                        .build();
                box.addContent(createFillArc(segmentStart, length, thickness, segmentDegrees[i]));
            }

            segmentStart = segmentEnd + SEGMENT_GAP_DEGREES;
        }

        float labelPosition = 0f;
        for (int i = 0; i < labels.length && i < segmentDegrees.length; i++) {
            labelPosition += segmentDegrees[i] + SEGMENT_GAP_DEGREES;
            box.addContent(createLabel(labels[i], labelPosition - SEGMENT_GAP_DEGREES / 2));
        }

        box.addContent(new LayoutElementBuilders.Column.Builder()
                .setHorizontalAlignment(LayoutElementBuilders.HORIZONTAL_ALIGN_CENTER)
                .addContent(createText(header, 13f, TEXT_MUTED, LayoutElementBuilders.FONT_WEIGHT_NORMAL))
                .addContent(createCountdown(endEpochSeconds))
                .addContent(createText(countdownCaption, 11f, TEXT_MUTED, LayoutElementBuilders.FONT_WEIGHT_NORMAL))
                .addContent(new LayoutElementBuilders.Spacer.Builder().setHeight(dp(10f)).build())
                .addContent(createText(footer, 13f, TEXT, LayoutElementBuilders.FONT_WEIGHT_NORMAL))
                .build());

        return box.build();
    }

    public static LayoutElementBuilders.LayoutElement createMessage(String message, String packageName, String activityClassName) {
        return createRoot(packageName, activityClassName)
                .addContent(createText(message, 14f, TEXT_MUTED, LayoutElementBuilders.FONT_WEIGHT_NORMAL))
                .build();
    }

    private static LayoutElementBuilders.Box.Builder createRoot(String packageName, String activityClassName) {
        return new LayoutElementBuilders.Box.Builder()
                .setWidth(expand())
                .setHeight(expand())
                .setModifiers(new ModifiersBuilders.Modifiers.Builder()
                        .setClickable(new ModifiersBuilders.Clickable.Builder()
                                .setId("open_app")
                                .setOnClick(new ActionBuilders.LaunchAction.Builder()
                                        .setAndroidActivity(new ActionBuilders.AndroidActivity.Builder()
                                                .setPackageName(packageName)
                                                .setClassName(activityClassName)
                                                .build())
                                        .build())
                                .build())
                        .build());
    }

    /** The whole prayer time, one piece per section between two sub times. */
    private static LayoutElementBuilders.LayoutElement createTrack(float[] segmentDegrees, float thickness) {
        LayoutElementBuilders.Arc.Builder arc = new LayoutElementBuilders.Arc.Builder()
                .setAnchorAngle(degrees(ARC_START_DEGREES))
                .setAnchorType(LayoutElementBuilders.ARC_ANCHOR_START);

        for (int i = 0; i < segmentDegrees.length; i++) {
            if (i > 0) {
                arc.addContent(new LayoutElementBuilders.ArcSpacer.Builder()
                        .setLength(degrees(SEGMENT_GAP_DEGREES))
                        .setThickness(dp(thickness))
                        .build());
            }

            arc.addContent(new LayoutElementBuilders.ArcLine.Builder()
                    .setLength(degrees(segmentDegrees[i]))
                    .setThickness(dp(thickness))
                    .setColor(ColorBuilders.argb(TRACK))
                    .build());
        }

        return arc.build();
    }

    /** A wait rather than a prayer time, so it is dotted instead of solid, just like on the phone. */
    private static LayoutElementBuilders.LayoutElement createDashedTrack(float thickness) {
        return new LayoutElementBuilders.Arc.Builder()
                .setAnchorAngle(degrees(ARC_START_DEGREES))
                .setAnchorType(LayoutElementBuilders.ARC_ANCHOR_START)
                .addContent(new LayoutElementBuilders.DashedArcLine.Builder()
                        .setLength(degrees(ARC_SWEEP_DEGREES))
                        .setThickness(thickness)
                        .setColor(ColorBuilders.argb(TRACK))
                        .setLinePattern(new LayoutElementBuilders.DashedLinePattern.Builder()
                                .setGapSize(6f)
                                .setGapInterval(4f)
                                .build())
                        .build())
                .build();
    }

    private static LayoutElementBuilders.LayoutElement createFillArc(
            float startDegrees,
            DimensionBuilders.DegreesProp length,
            float thickness,
            Float maxDegrees) {
        LayoutElementBuilders.ArcLine.Builder line = new LayoutElementBuilders.ArcLine.Builder()
                .setLength(length)
                .setThickness(dp(thickness))
                .setColor(ColorBuilders.argb(ACCENT));

        if (maxDegrees != null) {
            line.setLayoutConstraintsForDynamicLength(
                    new DimensionBuilders.AngularLayoutConstraint.Builder(maxDegrees).build());
        }

        return new LayoutElementBuilders.Arc.Builder()
                .setAnchorAngle(degrees(ARC_START_DEGREES + startDegrees))
                .setAnchorType(LayoutElementBuilders.ARC_ANCHOR_START)
                .addContent(line.build())
                .build();
    }

    /** The name of a sub time, written just inside the arc right where its section starts. */
    private static LayoutElementBuilders.LayoutElement createLabel(String label, float position) {
        return new LayoutElementBuilders.Box.Builder()
                .setWidth(expand())
                .setHeight(expand())
                .setModifiers(new ModifiersBuilders.Modifiers.Builder()
                        .setPadding(new ModifiersBuilders.Padding.Builder().setAll(dp(LABEL_INSET_DP)).build())
                        .build())
                .addContent(new LayoutElementBuilders.Arc.Builder()
                        .setAnchorAngle(degrees(ARC_START_DEGREES + position))
                        .setAnchorType(LayoutElementBuilders.ARC_ANCHOR_CENTER)
                        .addContent(new LayoutElementBuilders.ArcText.Builder()
                                .setText(label)
                                .setFontStyle(createFontStyle(11f, TEXT_MUTED, LayoutElementBuilders.FONT_WEIGHT_NORMAL))
                                .build())
                        .build())
                .build();
    }

    /** Counts down to the end on the watch itself, so it ticks every second. */
    private static LayoutElementBuilders.LayoutElement createCountdown(long endEpochSeconds) {
        DynamicInt32 remainingSeconds = DynamicInstant.platformTimeWithSecondsPrecision()
                .durationUntil(DynamicInstant.withSecondsPrecision(Instant.ofEpochSecond(endEpochSeconds)))
                .toIntSeconds();

        DynamicInt32.IntFormatter twoDigits = new DynamicInt32.IntFormatter.Builder().setMinIntegerDigits(2).build();

        DynamicString text = remainingSeconds.div(3600).format(twoDigits)
                .concat(DynamicString.constant(":"))
                .concat(remainingSeconds.rem(3600).div(60).format(twoDigits))
                .concat(DynamicString.constant(":"))
                .concat(remainingSeconds.rem(60).format(twoDigits));

        long fallback = Math.max(0, endEpochSeconds - Instant.now().getEpochSecond());
        String fallbackText = String.format("%02d:%02d:%02d", fallback / 3600, fallback % 3600 / 60, fallback % 60);

        return new LayoutElementBuilders.Text.Builder()
                .setText(new TypeBuilders.StringProp.Builder(fallbackText).setDynamicValue(text).build())
                .setLayoutConstraintsForDynamicText(new TypeBuilders.StringLayoutConstraint.Builder("00:00:00").build())
                .setFontStyle(createFontStyle(30f, TEXT, LayoutElementBuilders.FONT_WEIGHT_MEDIUM))
                .build();
    }

    /** Degrees of the arc which have passed by now, evaluated on the watch every second. */
    private static DynamicFloat getElapsedDegrees(long startEpochSeconds, long endEpochSeconds) {
        float degreesPerSecond = ARC_SWEEP_DEGREES / Math.max(1, endEpochSeconds - startEpochSeconds);

        return DynamicInstant.withSecondsPrecision(Instant.ofEpochSecond(startEpochSeconds))
                .durationUntil(DynamicInstant.platformTimeWithSecondsPrecision())
                .toIntSeconds()
                .asFloat()
                .times(degreesPerSecond);
    }

    /** Length of each section, the gaps between them taken out evenly. */
    private static float[] getSegmentDegrees(int[] segmentLengths) {
        float available = ARC_SWEEP_DEGREES - SEGMENT_GAP_DEGREES * (segmentLengths.length - 1);
        float[] result = new float[segmentLengths.length];

        for (int i = 0; i < segmentLengths.length; i++) {
            result[i] = available * segmentLengths[i] / 100f;
        }

        return result;
    }

    private static LayoutElementBuilders.LayoutElement createText(String text, float size, int color, int weight) {
        return new LayoutElementBuilders.Text.Builder()
                .setText(text)
                .setFontStyle(createFontStyle(size, color, weight))
                .build();
    }

    private static LayoutElementBuilders.FontStyle createFontStyle(float size, int color, int weight) {
        return new LayoutElementBuilders.FontStyle.Builder()
                .setSize(sp(size))
                .setColor(ColorBuilders.argb(color))
                .setWeight(weight)
                .build();
    }
}
