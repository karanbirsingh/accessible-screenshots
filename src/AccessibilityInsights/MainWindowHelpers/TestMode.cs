// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.Enums;
using AccessibilityInsights.Misc;
using Axe.Windows.Actions;
using Axe.Windows.Desktop.Settings;
using AccessibilityInsights.SharedUx.Telemetry;
using AccessibilityInsights.SharedUx.Dialogs;
using System;
using System.Globalization;
using AccessibilityInsights.SharedUx.Highlighting;
using AccessibilityInsights.CommonUxComponents.Dialogs;

namespace AccessibilityInsights
{
    /// <summary>
    /// MainWindow partial class for HandleTestStartUpRequest
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Start Test Mode
        /// </summary>
        /// <returns></returns>
        void StartTestMode(TestView view)
        {
            if (view != TestView.NoSelection)
            {
                if (SelectAction.GetDefaultInstance().IsPaused)
                {
                    ClearOverlayDriver.BringMainWindowOfPOIElementToFront();
                }
                HandlePauseButtonToggle(true);
            }
            switch (view)
            {
                case TestView.NoSelection:
                    StartTestNoSelection();
                    break;
                case TestView.AutomatedTestResults:
                    StartTestAutomatedChecksView();
                    break;
                case TestView.TabStop:
                    break;
                case TestView.ElementDetails:
                    StartElementDetailView();
                    break;
                case TestView.ElementHowToFix:
                    StartElementHowToFixView();
                    break;
            }
        }

        /// <summary>
        /// Start snapshot mode. 
        /// </summary>
        private void StartElementDetailView()
        {
            var ecId = SelectAction.GetDefaultInstance().GetSelectedElementContextId();
            if (ecId != null)
            {
                this.CurrentPage = AppPage.Test;
                this.CurrentView = TestView.CapturingData;
                DisableElementSelector();

                var tp = GetDataAction.GetProcessAndUIFrameworkOfElementContext(ecId.Value);

                PageTracker.TrackPage(this.CurrentPage, this.CurrentView.ToString(), tp.Item2);

                this.ctrlCurMode.HideControl();
                this.ctrlCurMode = this.ctrlSnapMode;
                this.ctrlSnapMode.DataContextMode = GetDataContextModeForTest();
                this.ctrlCurMode.ShowControl();

#pragma warning disable CS4014
                // NOTE: We aren't awaiting this async call, so if you
                // touch it, consider if you need to add the await
                ctrlSnapMode.SetElement(ecId.Value);
#pragma warning restore CS4014
            }
            else
            {
                this.AllowFurtherAction = false;
                //MessageDialog.Show(Properties.Resources.StartElementDetailViewNoElementIsSelectedMessage);
                this.AllowFurtherAction = true;
            }
        }

        /// <summary>
        /// Switches to snapshot mode and selects appropriate element
        /// </summary>
        /// <param name="followUp">An action to perform after tests have been run</param>
        private void StartElementHowToFixView(Action followUp = null)
        {
            var ecId = SelectAction.GetDefaultInstance().GetSelectedElementContextId();

            this.CurrentPage = AppPage.Test;
            this.CurrentView = TestView.ElementHowToFix;

            var tp = GetDataAction.GetProcessAndUIFrameworkOfElementContext(ecId.Value);
            PageTracker.TrackPage(this.CurrentPage, this.CurrentView.ToString(), tp.Item2);

            this.ctrlCurMode.HideControl();
            this.ctrlCurMode = this.ctrlSnapMode;
            this.ctrlSnapMode.DataContextMode = GetDataContextModeForTest();
            this.ctrlCurMode.ShowControl();

            DisableElementSelector();
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
            ctrlSnapMode.SetElement(ecId.Value).ContinueWith(
                t =>
                {
                    followUp?.Invoke();
                });
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler

        }

        /// <summary>
        /// Start test mode when no selection has been made and tests have not been run
        /// </summary>
        private void StartTestNoSelection()
        {
        }

        /// <summary>
        /// Start fastpass mode. 
        /// </summary>
        private void StartTestAutomatedChecksView()
        {
            
        }

        /// <summary>
        /// Allow to change view value from mode control and update title.
        /// </summary>
        /// <param name="view"></param>
        internal void SetCurrentViewAndUpdateUI(dynamic view)
        {
            var ec = SelectAction.GetDefaultInstance().GetSelectedElementContextId();

            this.CurrentView = view;
            if (ec != null)
            {
                var tp = GetDataAction.GetProcessAndUIFrameworkOfElementContext(ec.Value);
                PageTracker.TrackPage(this.CurrentPage, this.CurrentView.ToString(), tp.Item2);
            }

            UpdateTitleString();
            UpdateMainCommandButtons();
        }

        private void StartLoadingScreenshot(string path)
        {
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("path is not png");
            }

            var metadata = AccessibleScreenshot.LoadMetadata(path);

            DisableElementSelector();

            this.ctrlCurMode.HideControl();
            this.ctrlCurMode = this.ctrlSnapMode;
            /*
            this.ctrlSnapMode.DataContextMode = GetDataContextModeForTest();
            this.CurrentPage = AppPage.Test;
            this.CurrentView = TestView.ElementDetails;
            this.ctrlCurMode.ShowControl();
            */
        }

        /// <summary>
        /// Start snapshot mode with loading data
        /// </summary>
        private void StartLoadingSnapshot(string path, int? selectedElementId = null)
        {
            DisableElementSelector();

            var v = SelectAction.GetDefaultInstance().SelectLoadedData(path, selectedElementId);

            Logger.PublishTelemetryEvent(TelemetryEventFactory.ForLoadDataFile(v.Item2.Mode.ToString()));

            if (v.Item2.Mode == A11yFileMode.Test && !selectedElementId.HasValue)
            {
                StartTestAutomatedChecksView();
            }
            else if (v.Item2.Mode == A11yFileMode.Contrast)
            {
                StartTestAutomatedChecksView(); // we got rid of the CC test tab.
            }
            else // A11yFileMode.Inspect
            {
                this.ctrlCurMode.HideControl();
                this.ctrlCurMode = this.ctrlSnapMode;
                this.ctrlSnapMode.DataContextMode = GetDataContextModeForTest();
                this.CurrentPage = AppPage.Test;
                this.CurrentView = TestView.ElementDetails;
                this.ctrlCurMode.ShowControl();
#pragma warning disable CS4014
                // NOTE: We aren't awaiting this async call, so if you
                // touch it, consider if you need to add the await
                this.ctrlSnapMode.SetElement(v.Item1);
#pragma warning restore CS4014
            }

            PageTracker.TrackPage(this.CurrentPage, this.CurrentView.ToString());
        }
    }
}
