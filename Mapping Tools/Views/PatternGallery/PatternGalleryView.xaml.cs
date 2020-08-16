﻿using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.SystemTools.QuickRun;
using Mapping_Tools.Classes.Tools;
using Mapping_Tools.Classes.Tools.PatternGallery;
using Mapping_Tools.Viewmodels;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Mapping_Tools.Views.PatternGallery {
    /// <summary>
    /// Interactielogica voor PatternGalleryView.xaml
    /// </summary>
    [SmartQuickRunUsage(SmartQuickRunTargets.Always)]
    public partial class PatternGalleryView : ISavable<PatternGalleryVm>, IQuickRun
    {
        public string AutoSavePath => Path.Combine(MainWindow.AppDataPath, "patterngalleryproject.json");

        public string DefaultSaveFolder => Path.Combine(MainWindow.AppDataPath, "Pattern Gallery Projects");

        public static readonly string ToolName = "Pattern Gallery";
        public static readonly string ToolDescription =
            $@"Save and load patterns from osu! beatmaps.";

        /// <summary>
        /// 
        /// </summary>
        public PatternGalleryView()
        {
            InitializeComponent();
            DataContext = new PatternGalleryVm();
            Width = MainWindow.AppWindow.content_views.Width;
            Height = MainWindow.AppWindow.content_views.Height;
            ProjectManager.LoadProject(this, message: false);
            InitializeOsuPatternFileHandler();
        }

        public PatternGalleryVm ViewModel => (PatternGalleryVm)DataContext;

        public event EventHandler RunFinished;

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            e.Result = ExportPattern((PatternGalleryVm) e.Argument, bgw, e);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            RunTool(MainWindow.AppWindow.GetCurrentMaps(), quick: false);
        }

        public void QuickRun()
        {
            RunTool(new[] {IOHelper.GetCurrentBeatmapOrCurrentBeatmap()}, quick: true);
        }

        private void RunTool(string[] paths, bool quick = false)
        {
            if (!CanRun) return;

            // Remove logical focus to trigger LostFocus on any fields that didn't yet update the ViewModel
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);

            BackupManager.SaveMapBackup(paths);

            ViewModel.Paths = paths;
            ViewModel.Quick = quick;

            BackgroundWorker.RunWorkerAsync(DataContext);

            CanRun = false;
        }

        private string ExportPattern(PatternGalleryVm args, BackgroundWorker worker, DoWorkEventArgs _) {
            var reader = EditorReaderStuff.GetFullEditorReaderOrNot();
            var editor = EditorReaderStuff.GetNewestVersionOrNot(IOHelper.GetCurrentBeatmapOrCurrentBeatmap(), reader);

            var pattern = args.Patterns.FirstOrDefault(o => o.IsSelected);
            if (pattern == null)
                throw new Exception("No pattern has been selected to export.");

            var patternBeatmap = pattern.GetPatternBeatmap(args.FileHandler);

            var patternPlacer = new OsuPatternPlacer();
            if (reader != null) {
                patternPlacer.PlaceOsuPatternAtTime(patternBeatmap, editor.Beatmap, reader.EditorTime());
            } else {
                patternPlacer.PlaceOsuPattern(patternBeatmap, editor.Beatmap);
            }

            editor.SaveFile();

            // Increase pattern use count and time
            pattern.UseCount++;
            pattern.LastUsedTime = DateTime.Now;

            // Complete progressbar
            if (worker != null && worker.WorkerReportsProgress) worker.ReportProgress(100);

            // Do stuff
            if (args.Quick)
                RunFinished?.Invoke(this, new RunToolCompletedEventArgs(true, reader != null));

            return "Successfully exported pattern!";
        }

        private void InitializeOsuPatternFileHandler() {
            if (ViewModel.FileHandler == null) {
                ViewModel.FileHandler = new OsuPatternFileHandler(Path.Combine(DefaultSaveFolder, @"Pattern Files"));
            }
        }

        public PatternGalleryVm GetSaveData()
        {
            return (PatternGalleryVm) DataContext;
        }

        public void SetSaveData(PatternGalleryVm saveData)
        {
            DataContext = saveData;
            InitializeOsuPatternFileHandler();
        }
    }
}
