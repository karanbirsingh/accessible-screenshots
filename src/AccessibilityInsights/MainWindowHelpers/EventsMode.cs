// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.CommonUxComponents.Dialogs;
using AccessibilityInsights.Enums;
using AccessibilityInsights.Misc;
using AccessibilityInsights.SharedUx.Enums;
using AccessibilityInsights.SharedUx.Highlighting;
using Axe.Windows.Actions;
using Axe.Windows.Core.Bases;
using Axe.Windows.Desktop.UIAutomation;
using Axe.Windows.Desktop.UIAutomation.EventHandlers;
using System.Collections.Generic;

namespace AccessibilityInsights
{
    /// <summary>
    /// this is partial class for MainWindow
    /// this portion is for Events Mode
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Start snapshot mode. 
        /// </summary>
        /// <param name="e">root element for listening events</param>
        private void StartEventsMode(A11yElement e)
        {
        }

        /// <summary>
        /// Pause LiveMonitoring and get into Event mode for Loaded Events data
        /// </summary>
        /// <param name="path">path of event file. </param>
        private void StartEventsWithLoadedData(string path)
        {
        }
    }
}
