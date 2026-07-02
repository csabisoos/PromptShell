# PromptShell Fejlesztési Checklist (Plan.md)

Ez a dokumentum a PromptShell fejlesztésének lépésről lépésre követhető ellenőrzőlistája. A feladatok úgy lettek optimalizálva, hogy a logika és a UI párhuzamosan, de mégis jól elkülönítve fejlődhessen (MVVM, Clean Code).

## 1. Fázis: Alap Terminal Logika (Konzolos / Szerviz megközelítés)
- [ ] **Alapvető parancssori szolgáltatás (`ITerminalService`) létrehozása**
  - Készíts egy aszinkron szolgáltatást a `Services` mappában, amely a `System.Diagnostics.Process` segítségével elindítja a `zsh`-t. Tudnia kell fogadni egy string bemenetet, és visszaadni a `stdout` és `stderr` kimenetet.
- [ ] **Konzolos/Tesztes validálás (A UI nélkül)**
  - Mielőtt a UI-hoz kötnéd, teszteld a `TerminalService`-t tisztán egy ideiglenes konzolos belépési ponton, vagy a már meglévő `PromptShell.Tests` projektben. A cél, hogy a nyers parancsok (pl. `ls -la`) stabilan leofussanak, és visszatérjen a kimenet string formátumban, deadlockok nélkül.

## 2. Fázis: Avalonia Alapok (1.5)
- [ ] **UI és MVVM összekötése (`MainWindow.axaml` és `MainWindowViewModel`)**
  - Hozz létre egy egyszerű UI-t: egy beviteli mező (`TextBox`) a parancsnak, és egy csak olvasható, görgethető szövegdoboz (`TextBox` vagy `TextBlock` `ScrollViewer`-ben) a kimenetnek. Állítsd be a `CompiledBinding`-okat.
- [ ] **`ITerminalService` bekötése az Avalonia ablakba**
  - A ViewModel-ből (egy `[RelayCommand]` segítségével) hívjuk meg a terminál szolgáltatást. Így amit beírunk, az nyers parancsként lefut a terminálon, és a sima `plain output` egyből megjelenik a UI-on. Ez még *nem* AI, csak egy megbízható grafikus terminál wrapper.

## 3. Fázis: Bemenet Értelmezése AI-val (2)
- [ ] **Ollama HTTP Kliens (`IOllamaService`) integrálása**
  - A `HttpClient` felkészítése a lokális Ollama API (`http://localhost:11434/api/generate`) aszinkron hívására, JSON adatszerkezetek kezelésével.
- [ ] **AI parancsgenerálás logikájának (System Prompt) megírása**
  - A felhasználó emberi nyelvű bemenetét (pl. *"Mi az aktuális IP címem?"*) küldjük el az Ollamának. A System Promptnak rá kell kényszerítenie a modellt, hogy kizárólag a futtatható `zsh` parancsot adja vissza (pl. `curl ifconfig.me`), mindenféle "Here is your command" körítés nélkül.
- [ ] **Munkafolyamat összekapcsolása (Human -> AI -> Terminal -> Plain Output)**
  - A UI-ról érkező bemenetet először az `IOllamaService` kapja meg, a kitalált parancsot átadjuk az `ITerminalService`-nek, és a terminál *nyers* kimenetét megmutatjuk a felhasználónak.

## 4. Fázis: Avalonia Ablak Fejlesztése (2.5)
- [ ] **Aszinkron állapotok (Loading / Busy state) kezelése a UI-on**
  - Mivel az AI hívás és a parancsfuttatás másodpercekig tarthat, vezessünk be egy `IsBusy` property-t. Ennek segítségével tiltsuk le az input mezőt a folyamat idejére, és mutassunk egy `ProgressBar`-t vagy töltés indikátort.
- [ ] **Generált parancs előnézete (UI bővítés)**
  - Jelenítsük meg a felhasználónak, hogy pontosan milyen terminálparancsot talált ki az AI (pl. egy külön apró címkében), mielőtt az lefut. (Akár be is lehet tenni egy opcionális "Futtatás jóváhagyása" gombot a kritikus parancsokhoz).

## 5. Fázis: Output AI Elemzése (3)
- [ ] **Kimenet-értelmező logika (`IResultInterpreterService`) megírása**
  - A terminálból visszakapott nyers outputot és a kimeneti státuszkódot (exit code) visszaküldjük az Ollamának egy újabb HTTP kéréssel.
- [ ] **Emberi nyelvű válasz megjelenítése a UI-on**
  - Az AI elemzi a terminál outputját, és lefordítja emberi nyelvre (pl. *"A git push sikertelen volt, mert nincs beállítva az upstream ág. Használd a következőt: ..."*). Ezt a formázott, emberibb választ jelenítjük meg a felületen a nyers log helyett (vagy mellette, egy külön fülön/kártyán).

## 6. Fázis: Avalonia Ablak Véglegesítése (3.5)
- [ ] **Stílusok, Fluent téma és Dark Mode véglegesítése**
  - A PromptShell kapjon professzionális kinézetet. Monospace betűtípus (pl. Cascadia Code, JetBrains Mono) a terminálos elemekhez, szép paddingek, határozott szegélyek.
- [ ] **UX finomítások (Kényelmi funkciók)**
  - Command History (fel/le nyilakkal a korábbi kérések előhozása).
  - "Copy to Clipboard" (Vágólapra másolás) gomb az outputokhoz.
  - "Clear Console" gomb a képernyő letakarításához.

## 7. Fázis: Publish és Terjesztés (4)
- [ ] **Self-Contained Single File build elkészítése**
  - A `dotnet publish` megfelelő paraméterezése (`-r osx-arm64 --self-contained true /p:PublishSingleFile=true`), hogy az alkalmazás önmagában, telepítés nélkül is fusson a célgépen.
- [ ] **macOS `.app` Bundle struktúra kialakítása**
  - A kiadott bináris becsomagolása a szabványos macOS mapparendszerbe (`PromptShell.app/Contents/MacOS/`).
  - Az `Info.plist` fájl és az ikon (`.icns`) összekötése az operációs rendszerrel a professzionális asztali élményért.
