using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PipelineUtils
{
    // Stage in the Avatar Setup pipeline
    //   - Stage -1: find connected and active devices
    //   - Stage 0:  identify which tracked deviced is which: DeviceSetup.setupDevices
    //   - Stage 1:  set location of ankle joints: AvatarSetup.setupAnkleJoints
    //   - Stage 2:  set location of root joint: AvatarSetup.setupRootJoint
    //   - Stage 3:  set location of shoulder joints: AvatarSetup.setupShoulderJoints
    //   - Stage 4:  set location of wrist joints: AvatarSetup.setupWristJoints
    //   - Stage 5:  set location of neck joint: AvatarSetup.setupNeckJoint
    public enum Stage
    {
        DIRTY = -1,
        DEVICES = 0,
        T_POSE = 1,
        ANKLES = 2,
        ROOT = 3,
        LEGS = 4,
        SHOULDERS = 5,
        WRISTS = 6,
        NECK = 9,
        ROOT_AVATAR = 10,
        DONE = 11,
        ACCURACY_TESTING = 12
    }

    // Messages for the Avatar Setup pipeline
    public enum Language
    {
        ENGLISH = 0,
        SPANISH = 1,
        CATALAN = 2
    }
    public static Language currentLanguage = Language.ENGLISH;

    // Returns the next stage in the pipeline given the current stage, and present devices
    public static PipelineUtils.Stage nextStage(AvatarDriver driver, PipelineUtils.Stage current)
    {
        if (current == PipelineUtils.Stage.DEVICES)
        {
            return PipelineUtils.Stage.T_POSE;
        }
        if (current == PipelineUtils.Stage.T_POSE) 
        {
            if (driver.handLeft && driver.handLeft.activeInHierarchy &&
                driver.handRight && driver.handRight.activeInHierarchy)
            {
                return PipelineUtils.Stage.SHOULDERS;
            }
            else
            {
                current = PipelineUtils.Stage.SHOULDERS;
            }
        }
        if (current == PipelineUtils.Stage.SHOULDERS)
        {
            if (driver.handLeft && driver.handLeft.activeInHierarchy)
            {
                return PipelineUtils.Stage.NECK;
            }
            else
            {
                current = PipelineUtils.Stage.NECK;
            }
        }
        //if (current == PipelineUtils.Stage.WRISTS)
        //{
        //    if (driver.head && driver.head.activeInHierarchy)
        //    {
        //        return PipelineUtils.Stage.NECK;
        //    }
        //    else
        //    {
        //        current = PipelineUtils.Stage.NECK;
        //    }
        //}
        if (current == PipelineUtils.Stage.NECK)
        {
            if (driver.pelvis && driver.pelvis.activeInHierarchy)
            {
                return PipelineUtils.Stage.ROOT_AVATAR;
            }
            else
            {
                current = PipelineUtils.Stage.ROOT_AVATAR;
            }
        }
        if (current == PipelineUtils.Stage.ROOT_AVATAR)
        {
            return PipelineUtils.Stage.DONE;
        }
        if (current == PipelineUtils.Stage.DONE)
        {
            return PipelineUtils.Stage.DONE;
        }
        return PipelineUtils.Stage.DONE;
    }

    // Returns message to be shown before stage: i.e. instructions to user
    public static string introMessageAt(Stage current)
    {
        if (current == Stage.DIRTY)
        {
            return "";
        }
        else if (current == Stage.DEVICES)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Setting up device indices and taking some measures... Please, stand on a T-pose. Press TRIGGER when ready!";
            }
        }
        else if (current == Stage.T_POSE)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Taking some measures... Please, stand on a T-pose. Press TRIGGER when ready!";
            }
        }
        else if (current == Stage.ANKLES)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Setting up ankle joints... Please, stand on a Penguin-Pose with your Feet as a 'V'. Press TRIGGER when ready!";
            }
        }
        else if (current == Stage.ROOT)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Setting up root joint... Please, stand on a T-pose. Touch your hip from the front with a controller. Press TRIGGER when ready!";
            }
        }
        else if (current == Stage.LEGS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Setting up leg joints... Please, stand with arms akimbo. Place the hands where your legs begin. Press TRIGGER when ready!";
            }
        }
        else if (current == Stage.SHOULDERS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Stand on a T-pose. We need you to rotate your left and right arm around at the same time, and do not stop until told so. " +
                    "Press TRIGGER and start moving.";
            }
        }
        else if (current == Stage.WRISTS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Setting up left and right wrists joints... Please, stand on a T-pose. " +
                    "Press TRIGGER when in T-Pose.";
            }
        }
        else if (current == Stage.NECK)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "We need you to rotate your head around, and do not stop until told so. " +
                    "Press TRIGGER and start moving.";
            }
        }
        else if (current == Stage.ROOT_AVATAR)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "\n\n\n\n\nSetting up root... Please, stand on a T-pose inside the avatar shown. Press TRIGGER when ready!";
            }
        }
        else //if (current == Stage.DONE)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Avatar setup completed successfully.";
            }
        }
        return "";
    }

    // Returns message to be shown after completed stage: i.e. well done messages
    public static string successMessageAt(Stage current)
    {
        if (current == Stage.DIRTY)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "";
            }
        }
        else if (current == Stage.DEVICES)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Devices were identified correctly!";
            }
        }
        else if (current == Stage.T_POSE)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Measures were correctly captured!";
            }
        }
        else if (current == Stage.ANKLES)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Ankles were correctly placed!";
            }
        }
        else if (current == Stage.ROOT)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Root joint was correctly placed!";
            }
        }
        else if (current == Stage.LEGS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Leg joints were correctly placed!";
            }
        }
        else if (current == Stage.SHOULDERS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "You can stop now. Left and Right shoulders correctly placed!";
            }
        }
        else if (current == Stage.WRISTS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "You can stop now. Left and Right wrists correctly placed!";
            }
        }
        else if (current == Stage.NECK)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "You can stop now.";
            }
        }
        else //if (current == Stage.DONE)
        {
            return "";
        }
        return "";
    }

    // Returns message to be shown after failed stage: i.e. error messages
    public static string failureMessageAt(Stage current, int code = -1)
    {
        if (current == Stage.DIRTY)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Please, connect more controllers and/or trackers and press GRIP.";
            }
        }
        else if (current == Stage.DEVICES)
        {
            if (code == 0)
            {
                if (currentLanguage == Language.ENGLISH)
                {
                    return "Not enough devices! Need at least two controllers and/or trackers.";
                }
            }
            else if (code == 1)
            {
                if (currentLanguage == Language.ENGLISH)
                {
                    return "Could not identify tracked objects! Make sure you're standing on a T-pose.";
                }
            }
            else if (code == 2)
            {
                if (currentLanguage == Language.ENGLISH)
                {
                    return "Your head is not aligned with the rest of your body! Make sure you're standing on a T-pose.";
                }
            }
            return "";
        }
        else if (current == Stage.T_POSE)
        {
            return "";
        }
        else if (current == Stage.ANKLES)
        {
            return "";
        }
        else if (current == Stage.ROOT)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "We need at least one controller to set the root joint. Please connect a controller and try again.";
            }
        }
        else if (current == Stage.LEGS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "We need at least one controller to set the leg joints. Please connect a controller and try again.";
            }
        }
        else if (current == Stage.SHOULDERS)
        {
            if (code == 0)
            {
                if (currentLanguage == Language.ENGLISH)
                {
                    return "We need both controllers to set the left shoulder joint. Please connect a controller and try again.";
                }
            }
            else if (code == 1)
            {
                if (currentLanguage == Language.ENGLISH)
                {
                    return "You can stop now. Wrong movement detected. We need to do it again.";
                }
            }
        }
        else if (current == Stage.WRISTS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "You can stop now. Wrong movement detected. We need to do it again.";
            }
        }
        else if (current == Stage.NECK)
        {
            if (code == 0)
            {
                if (currentLanguage == Language.ENGLISH)
                {
                    return "We need both controllers to set the neck joint. Please connect a controller and try again.";
                }
            }
            else if (code == 1)
            {
                if (currentLanguage == Language.ENGLISH)
                {
                    return "You can stop now. Wrong movement detected. We need to do it again.";
                }
            }
        }
        else //if (current == Stage.DONE)
        {
            return "";
        }
        return "";
    }

    // Returns message to be shown while executing stages: i.e. progress messages
    public static string progressMessageAt(Stage current)
    {
        if (current == Stage.DIRTY)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                return "Found {0} controller(s) and {1} tracker(s).";
            }
        }
        else if (current == Stage.DEVICES)
        {
            return "";
        }
        else if (current == Stage.T_POSE)
        {
            return "Taking some measures... Please, stand on a T-pose.";
        }
        else if (current == Stage.ANKLES)
        {
            return "Setting up ankle joints... Please, stand on a Penguin-Pose with your Feet as a 'V'.";
        }
        else if (current == Stage.ROOT)
        {
            return "";
        }
        else if (current == Stage.LEGS)
        {
            return "";
        }
        else if (current == Stage.ROOT_AVATAR)
        {
            return "Setting up root... Please, stand on a T-pose inside the avatar shown.";
        }
        else if (current == Stage.SHOULDERS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                //return "Recording left and right arm: {0:0.00}% completed\n";
                return "Recording left and right arm: {0} points\n";
            }
        }
        else if (current == Stage.WRISTS)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                //return "Recording left and right hand: {0:0.00}% completed\n";
                return "Recording left and right hand: {0} points\n";
            }
        }
        else if (current == Stage.NECK)
        {
            if (currentLanguage == Language.ENGLISH)
            {
                //return "Recording head: {0:0.00}% completed\n";
                return "Recording head: {0} points\n";
            }
        }
        else //if (current == Stage.DONE)
        {
            return "";
        }
        return "";
    }

    // Displays success message followed by intro message for current transition
    public static void displayInBetweenStagesMessage(AvatarVR avatarVR, DisplayMirror displayMirror, PipelineUtils.Stage current, PipelineUtils.Stage next)
    {
        if (displayMirror == null) return;

        string message1 = PipelineUtils.successMessageAt(current);
        Color color1 = new Color(0.0f, 1.0f, 0.0f, 0.5f);
        int secs1 = 2;
        string message2 = PipelineUtils.introMessageAt(next);
        Color color2 = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        int secs2 = 0;
        if (next == PipelineUtils.Stage.DONE)
        {
            displayMirror.CleanText();
            color2 = new Color(0.0f, 1.0f, 0.0f, 0.5f);
            secs2 = 2;
        }
        //Debug.Log(message1 + "\n");
        //Debug.Log(message2 + "\n");
        if (displayMirror && displayMirror.isActiveAndEnabled)
        {
            displayMirror.ShowTextAgain(message1, color1, secs1, message2, color2, secs2, true);
        }
    }
}
