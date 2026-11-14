# PrintScreen

Zrzut ekranu ze wszystkich monitorów do jednego pliku PNG — w takim układzie, w jakim monitory są rozmieszczone w ustawieniach Windows.

Jeden plik `Screenshot.exe`, bez instalacji i bez okna konsoli. Przed zrzutem na głównym monitorze pojawia się odliczanie (domyślnie 10 s), które znika tuż przed zrobieniem zdjęcia.

## Użycie

Dwuklik na `Screenshot.exe`. Plik trafia do `Obrazy\Screenshots\Screenshot_<data>_<godzina>.png`.

| Przełącznik | Działanie |
| --- | --- |
| `-Delay 5` | odliczanie 5 s zamiast 10 (`-Delay 0` = bez odliczania) |
| `-Folder "D:\Zrzuty"` | zapis do wskazanego folderu |
| `-Open` | otwiera plik po zapisaniu |
| `-Clipboard` | dodatkowo kopiuje obraz do schowka |

Przełączniki można wpisać w skrócie do exe (pole „Element docelowy”). Skrótowi można też przypisać klawisz we Właściwościach.

## Jak to działa

- Obsługuje dowolną liczbę monitorów, także ustawionych na lewo lub powyżej głównego (ujemne współrzędne).
- Uwzględnia skalowanie ekranu (125%, 150%…), więc zrzut ma pełną rozdzielczość.
- Puste miejsca przy monitorach o różnych rozmiarach są czarne.

## Kompilacja

Wystarczy kompilator wbudowany w Windows (.NET Framework 4):

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /optimize /out:Screenshot.exe Screenshot.cs
```
