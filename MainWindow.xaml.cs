using NAudio.Wave; // Für Mp3FileReader und WaveFormat
using NAudio.Lame; // Für LameMP3FileWriter
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel; // Für INotifyPropertyChanged
using System.Windows.Media; // Für Farben im UI
using Microsoft.WindowsAPICodePack.Dialogs; // Für CommonOpenFileDialog
using System.Threading.Tasks; // Für Task.Run und Task.Delay

namespace Mp3_Merger
{
    public partial class MainWindow : Window
    {
        // ObservableCollection für die Liste der Album-Ordner
        public ObservableCollection<FolderInfo> FoldersToProcess { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            FoldersToProcess = new ObservableCollection<FolderInfo>();
            lstFolders.ItemsSource = FoldersToProcess; // Binden der Liste an die ListBox
        }

        // Hilfsklasse zum Speichern von Ordnerpfad und Status
        public class FolderInfo : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private int _index;
            public int Index
            {
                get { return _index; }
                set
                {
                    if (_index != value)
                    {
                        _index = value;
                        OnPropertyChanged(nameof(Index));
                    }
                }
            }

            public string FullPath { get; set; }
            public string FolderName => Path.GetFileName(FullPath);

            private string _status;
            public string Status
            {
                get { return _status; }
                set
                {
                    if (_status != value)
                    {
                        _status = value;
                        OnPropertyChanged(nameof(Status));
                        OnPropertyChanged(nameof(StatusColor)); // Status-Farbe aktualisieren
                    }
                }
            }

            // Liste der gefundenen Tracks
            private ObservableCollection<string> _tracks;
            public ObservableCollection<string> Tracks
            {
                get { return _tracks; }
                set
                {
                    if (_tracks != value)
                    {
                        _tracks = value;
                        OnPropertyChanged(nameof(Tracks));
                        OnPropertyChanged(nameof(TrackCountText)); // Anzahl der Tracks aktualisieren
                    }
                }
            }

            public string TrackCountText => Tracks != null ? $"({Tracks.Count} Tracks)" : "(Ordner scannen...)";

            // Für visuelles Feedback des Status
            public Brush StatusColor
            {
                get
                {
                    switch (Status)
                    {
                        case "Erfolgreich": return Brushes.DarkGreen;
                        case "Fehler": return Brushes.Red;
                        case "Verarbeite...": return Brushes.Blue;
                        case "Keine MP3-Dateien gefunden!": return Brushes.OrangeRed;
                        case "Überspringen (Datei existiert)": return Brushes.Gray;
                        case "Scanne MP3s...": return Brushes.Orange;
                        default: return Brushes.LightGray;
                    }
                }
            }

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void UpdateFolderIndexes()
        {
            for (int i = 0; i < FoldersToProcess.Count; i++)
            {
                FoldersToProcess[i].Index = i + 1;
            }
        }

        // --- Event-Handler für Buttons und Drag & Drop ---

        private async void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string selectedFolder = dialog.FileName;
                    if (!FoldersToProcess.Any(f => f.FullPath == selectedFolder))
                    {
                        var newFolderInfo = new FolderInfo { FullPath = selectedFolder, Status = "Scanne MP3s...", Tracks = new ObservableCollection<string>() };
                        FoldersToProcess.Add(newFolderInfo);
                        UpdateFolderIndexes();

                        await System.Threading.Tasks.Task.Run(() => ScanFolderForMp3s(newFolderInfo));
                    }
                }
            }
        }

        // Methode zum Scannen von MP3s in einem Ordner
        private void ScanFolderForMp3s(FolderInfo folderInfo)
        {
            try
            {
                var mp3Files = Directory.GetFiles(folderInfo.FullPath, "*.mp3")
                                         .OrderBy(f => Path.GetFileName(f))
                                         .Select(f => Path.GetFileName(f))
                                         .ToList();

                Dispatcher.Invoke(() =>
                {
                    folderInfo.Tracks = new ObservableCollection<string>(mp3Files);
                    if (mp3Files.Count == 0)
                    {
                        folderInfo.Status = "Keine MP3-Dateien gefunden!";
                    }
                    else
                    {
                        folderInfo.Status = "Bereit";
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    folderInfo.Status = $"Fehler beim Scannen: {ex.Message}";
                    folderInfo.Tracks = new ObservableCollection<string>();
                });
            }
        }

        private void btnRemoveSelectedFolders_Click(object sender, RoutedEventArgs e)
        {
            if (lstFolders.SelectedItems.Count > 0)
            {
                var itemsToRemove = lstFolders.SelectedItems.Cast<FolderInfo>().ToList();
                foreach (var item in itemsToRemove)
                {
                    FoldersToProcess.Remove(item);
                }
                UpdateFolderIndexes();
            }
        }

        // Drag & Drop: Ordner in die ListBox ziehen
        private async void lstFolders_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string itemPath in droppedItems)
                {
                    if (Directory.Exists(itemPath))
                    {
                        if (!FoldersToProcess.Any(f => f.FullPath == itemPath))
                        {
                            var newFolderInfo = new FolderInfo { FullPath = itemPath, Status = "Scanne MP3s...", Tracks = new ObservableCollection<string>() };
                            FoldersToProcess.Add(newFolderInfo);
                            UpdateFolderIndexes();
                            await System.Threading.Tasks.Task.Run(() => ScanFolderForMp3s(newFolderInfo));
                        }
                    }
                }
            }
        }

        private void lstFolders_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        // --- HAUPT-VERARBEITUNGSLOGIK FÜR ALLE ORDNER ---
        private async void btnStartProcessing_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersToProcess.Count == 0)
            {
                MessageBox.Show("Bitte fügen Sie zuerst Album-Ordner zur Liste hinzu.", "Keine Ordner ausgewählt", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sicherstellen, dass ProgressBar und Status-Text sichtbar sind
            progressBarOverall.Visibility = Visibility.Visible;
            txtStatusOverall.Visibility = Visibility.Visible;

            txtStatusOverall.Text = "Verarbeitung gestartet..."; // Initialer Status-Text

            // Deaktiviere alle relevanten Buttons während der Verarbeitung
            btnStartProcessing.IsEnabled = false;
            btnAddFolder.IsEnabled = false;
            btnRemoveSelectedFolders.IsEnabled = false;

            // Die ProgressBar zeigt jetzt den Fortschritt INNERHALB eines Ordners an
            // Der Maximum-Wert wird dynamisch gesetzt
            progressBarOverall.Value = 0;

            int processedFolderCount = 0; // Zählt die komplett verarbeiteten Ordner

            // Erstelle eine Kopie der Liste, um Modifikationen während der Iteration zu vermeiden
            // (Obwohl wir keine Elemente entfernen, ist es gute Praxis bei async-Operationen)
            foreach (var folderInfo in FoldersToProcess.ToList())
            {
                // Update des Haupt-Status für den aktuellen Ordner
                Dispatcher.Invoke(() =>
                {
                    txtStatusOverall.Text = $"Verarbeite Album: {processedFolderCount + 1}/{FoldersToProcess.Count} - {folderInfo.FolderName}";
                    progressBarOverall.Value = 0; // Setze ProgressBar für jeden Ordner zurück
                });


                if (folderInfo.Tracks == null || folderInfo.Tracks.Count == 0)
                {
                    folderInfo.Status = "Fehler: Keine Tracks gefunden oder noch nicht gescannt!";
                    processedFolderCount++;
                    Dispatcher.Invoke(() => lstFolders.Items.Refresh());
                    await Task.Delay(50); // Kleine Pause für UI-Update
                    continue;
                }

                var actualMp3FilePaths = folderInfo.Tracks.Select(t => Path.Combine(folderInfo.FullPath, t)).ToList();

                string outputFileName = $"{folderInfo.FolderName}.mp3";
                string outputPath = Path.Combine(folderInfo.FullPath, outputFileName);

                if (File.Exists(outputPath))
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"Die Datei '{outputFileName}' existiert bereits im Ordner '{folderInfo.FolderName}'. Überschreiben?",
                        "Datei existiert",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );
                    if (result == MessageBoxResult.No)
                    {
                        folderInfo.Status = "Überspringen (Datei existiert)";
                        processedFolderCount++;
                        Dispatcher.Invoke(() => lstFolders.Items.Refresh());
                        await Task.Delay(50); // Kleine Pause für UI-Update
                        continue;
                    }
                }

                try
                {
                    folderInfo.Status = "Verarbeite...";
                    Dispatcher.Invoke(() => lstFolders.Items.Refresh());
                    await Task.Delay(50); // Kleine Pause für UI-Update

                    // Erstelle ein Progress-Objekt für die Fortschrittsmeldung
                    var progress = new Progress<string>(update =>
                    {
                        // Dieser Code läuft wieder auf dem UI-Thread
                        txtStatusOverall.Text = update;
                    });

                    // Setze den Maximum-Wert der ProgressBar auf die Anzahl der Tracks im aktuellen Ordner
                    Dispatcher.Invoke(() =>
                    {
                        progressBarOverall.Maximum = actualMp3FilePaths.Count;
                    });

                    await System.Threading.Tasks.Task.Run(() => MergeMp3Files(actualMp3FilePaths, outputPath, folderInfo, progress));
                    folderInfo.Status = "Erfolgreich";
                }
                catch (Exception ex)
                {
                    folderInfo.Status = $"Fehler: {ex.Message}";
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Fehler beim Verarbeiten von '{folderInfo.FolderName}': {ex.Message}", "Verarbeitungsfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                finally
                {
                    processedFolderCount++; // Erhöht erst hier, wenn der Ordner durch ist.
                    Dispatcher.Invoke(() =>
                    {
                        lstFolders.Items.Refresh(); // Stellt sicher, dass die Status-Farben und Texte in der Liste aktualisiert werden
                        // Wenn der Ordner fertig ist, setzen wir die ProgressBar auf 100%, bevor der nächste Ordner beginnt
                        progressBarOverall.Value = progressBarOverall.Maximum;
                    });
                }
            }

            // Nach Abschluss aller Verarbeitungen
            txtStatusOverall.Text = "Alle aufgeführten Alben erfolgreich verarbeitet!"; // Finaler Erfolgs-Text
            progressBarOverall.Value = progressBarOverall.Maximum; // Sicherstellen, dass ProgressBar 100% anzeigt

            bool allSuccessful = FoldersToProcess.All(f => f.Status == "Erfolgreich" || f.Status == "Überspringen (Datei existiert)");
            if (allSuccessful)
            {
                MessageBox.Show("Alle aufgeführten Alben wurden erfolgreich verarbeitet.", "Verarbeitung abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Die Verarbeitung wurde abgeschlossen. Einige Alben wurden mit Fehlern verarbeitet oder übersprungen. Details siehe Liste.", "Verarbeitung abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Buttons wieder aktivieren
            btnStartProcessing.IsEnabled = true;
            btnAddFolder.IsEnabled = true;
            btnRemoveSelectedFolders.IsEnabled = true;

            // Optional: ProgressBar und Status-Text nach einer kurzen Verzögerung ausblenden
            // await Task.Delay(3000); // Warte 3 Sekunden
            // progressBarOverall.Visibility = Visibility.Collapsed;
            // txtStatusOverall.Visibility = Visibility.Collapsed;
        }

        private void MergeMp3Files(List<string> inputFiles, string outputFile, FolderInfo currentFolderInfo, IProgress<string> progress)
        {
            WaveFormat outputFormat;
            try
            {
                // Bestimme das Audioformat der ersten Datei
                using (var reader = new Mp3FileReader(inputFiles.First()))
                {
                    outputFormat = reader.WaveFormat;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler beim Lesen des Formats der ersten MP3-Datei im Ordner '{currentFolderInfo.FolderName}': {ex.Message}");
            }

            using (var writer = new LameMP3FileWriter(outputFile, outputFormat, 320))
            {
                for (int i = 0; i < inputFiles.Count; i++)
                {
                    string inputFile = inputFiles[i];
                    try
                    {
                        // Fortschritt melden
                        // "% der Tracks innerhalb dieses Albums"
                        // Aktueller Track X von Y: "Name des Tracks"
                        string statusMessage = $"Album {currentFolderInfo.FolderName}: Track {i + 1}/{inputFiles.Count} - {Path.GetFileName(inputFile)} wird gemerged...";
                        progress.Report(statusMessage);

                        // Aktualisiere ProgressBar pro Track
                        Dispatcher.Invoke(() => progressBarOverall.Value = i + 1);

                        using (var reader = new Mp3FileReader(inputFile))
                        {
                            if (!reader.WaveFormat.Equals(outputFormat))
                            {
                                throw new InvalidOperationException($"Die Datei '{Path.GetFileName(inputFile)}' im Ordner '{currentFolderInfo.FolderName}' hat ein inkompatibles Audioformat ({reader.WaveFormat}). Erwartet: {outputFormat}.");
                            }
                            reader.CopyTo(writer); // Kopiert den Audio-Stream der aktuellen Datei in die Ausgabedatei
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Fehler beim Verarbeiten von Track '{Path.GetFileName(inputFile)}' im Ordner '{currentFolderInfo.FolderName}': {ex.Message}", ex);
                    }
                }
            }
        }
    }
}