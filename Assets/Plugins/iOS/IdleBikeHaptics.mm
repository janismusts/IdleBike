#import <UIKit/UIKit.h>

extern "C" {

void _ibHapticImpact(int style)
{
    if (@available(iOS 10.0, *))
    {
        UIImpactFeedbackStyle s = UIImpactFeedbackStyleLight;
        if (style == 1) s = UIImpactFeedbackStyleMedium;
        else if (style == 2) s = UIImpactFeedbackStyleHeavy;
        UIImpactFeedbackGenerator *gen = [[UIImpactFeedbackGenerator alloc] initWithStyle:s];
        [gen prepare];
        [gen impactOccurred];
    }
}

void _ibHapticSelection(void)
{
    if (@available(iOS 10.0, *))
    {
        UISelectionFeedbackGenerator *gen = [[UISelectionFeedbackGenerator alloc] init];
        [gen prepare];
        [gen selectionChanged];
    }
}

}
