# MP3 Album Batch Merger

Eine einfache und effiziente Desktop-Anwendung, entwickelt mit C# und WPF, zum schnellen Zusammenführen mehrerer MP3-Dateien eines Albums zu einer einzigen MP3-Datei. Ideal für das Archivieren von Alben oder das Erstellen von nahtlosen Mixen.

## 🎵 Was macht diese Software?

Der MP3 Album Batch Merger ist ein praktisches Tool für Musikliebhaber und Audiophiler. Er löst das Problem, wenn Sie viele einzelne MP3-Tracks eines Albums haben und diese zu einer einzigen, großen MP3-Datei zusammenführen möchten. Dies ist besonders nützlich für:

- **Einfache Archivierung:** Speichern Sie ein komplettes Album als eine einzige Datei, um den Speicherplatz zu optimieren und die Verwaltung zu vereinfachen.

- **Nahtlose Wiedergabe:** Erstellen Sie MP3-Dateien ohne Pausen oder Unterbrechungen zwischen den Tracks, ideal für Klassik-Alben, Live-Aufnahmen oder DJ-Mixe.

- **Portabilität:** Eine einzige Datei ist einfacher auf mobile Geräte, USB-Sticks oder ältere Mediaplayer zu übertragen.

Die Anwendung ermöglicht es Ihnen, mehrere Album-Ordner gleichzeitig zu verarbeiten und bietet dabei detailliertes Feedback zum Fortschritt.

![image](https://github.com/user-attachments/assets/8b287ae3-c854-497a-b80f-1f2fca59668a)

## ✨ Features

- **Batch-Verarbeitung:** Fügen Sie mehrere Album-Ordner zur Liste hinzu und verarbeiten Sie diese in einem Rutsch.

- **Drag & Drop:** Ziehen Sie Ihre Album-Ordner einfach per Drag & Drop in die Anwendung.

- **Fortschrittsanzeige:** Eine detaillierte Fortschrittsleiste und Statusmeldungen informieren Sie über den aktuellen Bearbeitungsstand jedes Albums und Tracks.

- **Visuelles Feedback:** Ordnerstatus (bereit, in Bearbeitung, erfolgreich, Fehler) wird farblich hervorgehoben.

- **Track-Details:** Klappen Sie Ordnerdetails auf, um alle erkannten MP3-Dateien zu sehen. Nach der Verarbeitung wird die neu erstellte Gesamt-MP3-Datei ebenfalls in den Details angezeigt.

- **Überschreibschutz:** Warnung, wenn eine gemergte Datei bereits existiert, mit der Option zum Überspringen oder Überschreiben.

- **Intuitives UI:** Eine klare und benutzerfreundliche Oberfläche macht die Bedienung kinderleicht.

## 🛠️ Wie es funktioniert

Die Anwendung verwendet die leistungsstarke NAudio-Bibliothek, um MP3-Dateien zu lesen und zu schreiben. Sie iteriert durch die MP3-Dateien in jedem ausgewählten Ordner, verknüpft deren Audio-Streams und schreibt sie als eine einzige, qualitativ hochwertige MP3-Datei in denselben Ordner zurück. Dabei werden die originalen Dateinamen der Tracks verwendet, um die Reihenfolge der Zusammenführung zu bestimmen.

## 🚀 Erste Schritte
Herunterladen / Klonen: Laden Sie die neueste Version der Anwendung herunter oder klonen Sie dieses Repository.

- **Starten:** Führen Sie die ausführbare Datei aus.

- **Ordner hinzufügen:** Klicken Sie auf "Album-Ordner hinzufügen..." oder ziehen Sie Album-Ordner (die MP3-Dateien enthalten) per Drag & Drop in die Liste.

- **Verarbeiten:** Sobald Sie alle gewünschten Ordner hinzugefügt haben, klicken Sie auf "Alle aufgeführten Alben verarbeiten!", um den Merge-Vorgang zu starten.

## ⚠️ Wichtige Hinweise

- Stellen Sie sicher, dass sich alle MP3-Dateien eines Albums in einem dedizierten Ordner befinden, den Sie auswählen.

- Die Anwendung versucht, das Audioformat des ersten Tracks zu übernehmen. Bei sehr unterschiedlichen Formaten innerhalb eines Albums kann es zu Fehlern kommen.

- Die gemergte Datei wird im selben Ordner wie die Original-MP3s gespeichert und erhält den Namen des Ordners (z.B. MeinAlbum.mp3).

## 🤝 Mitwirken
Dieses Projekt ist für den persönlichen Gebrauch gedacht, aber Vorschläge zur Verbesserung oder Fehlerberichte sind immer willkommen!
