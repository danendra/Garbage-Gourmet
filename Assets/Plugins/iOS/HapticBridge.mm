#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator* _lightGenerator = nil;
static UIImpactFeedbackGenerator* _mediumGenerator = nil;
static UIImpactFeedbackGenerator* _heavyGenerator = nil;
static UINotificationFeedbackGenerator* _notificationGenerator = nil;

extern "C" {
    void _PlayImpactHaptic(int style) {
        dispatch_async(dispatch_get_main_queue(), ^{
            if (@available(iOS 10.0, *)) {
                UIImpactFeedbackGenerator* generator = nil;
                switch (style) {
                    case 0: // Light
                        if (_lightGenerator == nil) {
                            _lightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                        }
                        generator = _lightGenerator;
                        break;
                    case 1: // Medium
                        if (_mediumGenerator == nil) {
                            _mediumGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                        }
                        generator = _mediumGenerator;
                        break;
                    case 2: // Heavy
                        if (_heavyGenerator == nil) {
                            _heavyGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
                        }
                        generator = _heavyGenerator;
                        break;
                }
                
                if (generator != nil) {
                    [generator prepare];
                    [generator impactOccurred];
                }
            }
        });
    }

    void _PlayNotificationHaptic(int type) {
        dispatch_async(dispatch_get_main_queue(), ^{
            if (@available(iOS 10.0, *)) {
                if (_notificationGenerator == nil) {
                    _notificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
                }
                [_notificationGenerator prepare];
                
                switch (type) {
                    case 0: // Success
                        [_notificationGenerator notificationOccurred:UINotificationFeedbackTypeSuccess];
                        break;
                    case 1: // Warning
                        [_notificationGenerator notificationOccurred:UINotificationFeedbackTypeWarning];
                        break;
                    case 2: // Error
                        [_notificationGenerator notificationOccurred:UINotificationFeedbackTypeError];
                        break;
                }
            }
        });
    }
}
