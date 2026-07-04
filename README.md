# CarTrace


## 1. Descriere generală

CarTrace este o aplicație desktop de diagnoză auto, dezvoltată în limbajul C# pe platforma .NET Framework, care comunică cu unitatea de comandă a motorului (ECU) prin intermediul unui adaptor bazat pe circuitul integrat ELM327, conectat prin Bluetooth. Aplicația utilizează standardul OBD-II (specificația SAE J1979) pentru interpretarea parametrilor de funcționare ai vehiculului.

**Funcționalități principale:**

- citirea în timp real a parametrilor de funcționare (live data);
- citirea și ștergerea codurilor de eroare (DTC);
- interogarea cadrului înghețat (Freeze Frame) asociat unei defecțiuni;
- identificarea vehiculului prin seria de șasiu (VIN) și codurile de calibrare (Cal ID, CVN);
- măsurarea performanțelor de accelerație prin interpolare liniară;
- persistența și consultarea istoricului sesiunilor într-o bază de date în cloud (Supabase / PostgreSQL), organizată după VIN.

---

## 2. Tehnologii și cerințe

### Software

| | |
|---|---|
| **Sistem de operare** | Microsoft Windows 10 / 11 |
| **Framework** | .NET 4.7.2|
| **Limbaj** | C# 8.0 |
| **Interfață grafică** | Windows Forms |
| **Mediu de dezvoltare** | Microsoft Visual Studio 2022 |

### Dependențe externe (pachete NuGet)

| | |
|---|---|
| **Supabase** | client oficial .NET |
| **Postgrest** | client asincron pentru API-ul REST |

### Backend cloud

Supabase Cloud Infrastructure, motor relațional PostgreSQL, interfațat prin clientul asincron Postgrest.

### Hardware

- Adaptor ELM327 conectat la portul fizic J1962, comunicând prin profilul SPP peste Bluetooth, la o rată nominală de 115200 bps;
- Vehicul compatibil cu standardul OBD-II (SAE J1979).

---

## 3. Arhitectura aplicației

Arhitectura respectă principiul separării responsabilităților, organizată pe straturi:

```
LICENTA.bun.sln                  <- fișierul soluție (Visual Studio)
LICENTA.bun/
│
├── Data/
│   └── CommandRepository.cs     <- nomenclatorul centralizat al PID-urilor
│
├── Hardware/
│   └── ScannerOBD.cs            <- gestiunea de nivel jos a portului serial
│
├── Input_Output/
│   ├── CsvLogger.cs             <- scriere secvențială locală
│   └── FileManager.cs           <- generare rapoarte pe Desktop
│
├── Services/
│   ├── CommandProcessor.cs      <- dispecer comenzi AT / OBD
│   ├── DiagnosticsService.cs    <- citire/ștergere DTC
│   ├── DtcParser.cs             <- parser bitwise pentru codurile de eroare
│   ├── FreezeFrameService.cs    <- scanare cadru înghețat
│   ├── GraphDataParser.cs       <- extractor numeric
│   ├── ObdTranslator.cs         <- motor de decodare PID
│   ├── PerformanceService.cs    <- interpolare temporală
│   ├── Service09Decoder.cs      <- decodor multi-frame
│   └── VehicleInfoService.cs    <- agregare metadate vehicul
│
└── UI/
    ├── Form1.cs                 <- logica ferestrei principale
    ├── Form1.ModernUI.cs        <- generarea interfeței vizuale și evenimente
    ├── Program.cs               <- punctul de intrare (STAThread)
    └── UIConfigurator.cs        <- configurarea temei grafice
```

---

## 4. Compilare și lansare

### Cerințe prealabile

- **Microsoft Visual Studio 2022** cu componenta „.NET desktop development" și suport pentru .NET Framework 4.7.2;
- Adaptor ELM327 cu Bluetooth, asociat în **Setări Windows → Bluetooth** (sistemul îi alocă automat un port COM virtual, ex. COM7);
- Vehicul cu contactul pus (poziția ACC sau motorul pornit).

### Pași de lansare a aplicației

1. Se accesează repository-ul la adresa [https://github.com/Maco202020/CarTrace](https://github.com/Maco202020/CarTrace), se apasă butonul **Code → Download ZIP**. Arhiva descărcată se dezarhivează într-un director local.
2. Se deschide fișierul soluție **LICENTA.bun.sln** din folderul dezarhivat (dublu-click — se deschide automat în Visual Studio).
3. Se selectează configurația de compilare **Debug** și platforma **Any CPU**.
4. Se apasă **Run (▶)** sau tasta **F5**. Visual Studio compilează soluția și lansează aplicația.

> **Notă:** pachetele NuGet se restaurează automat la prima compilare. Dacă apare o eroare de tip „package missing", click dreapta pe soluție în Solution Explorer → **Restore NuGet Packages**.

### Utilizare inițială

1. Din lista **Select Port** se alege portul COM al adaptorului ELM327 (ex. COM7).
2. Se apasă butonul **CONNECT**. La reușită, butonul afișează „CONNECTED (COMx)" și aplicația extrage automat datele de identificare ale vehiculului (VIN, ECU Name, Calibration ID, CVN).
3. Se accesează panourile de diagnoză:
   - **Extract** — consolă de comenzi OBD cu răspuns RAW HEX și DECODED;
   - **Monitoring** — grafic în timp real al unui parametru selectat;
   - **Error Diagnosis** — citire și ștergere coduri DTC;
   - **Freeze Frame** — parametrii salvați de ECU la momentul apariției erorii;
   - **Performance** — test de accelerație 0–50 / 0–100 / 0–130 km/h;
   - **Order History** — istoricul sesiunilor din cloud, filtrat după VIN.

---

## 5. Fluxul de date

Fluxul unei comenzi de tip citire parametru (exemplu: turația motorului, PID `010C`) este ilustrat în diagrama de secvență din documentație. Pe scurt:

1. Utilizatorul lansează o comandă din interfață (`Form1`).
2. `Form1` apelează `CommandProcessor.ExecuteAsync("010C")`.
3. `CommandProcessor` identifică prefixul (diferit de `AT` ⟹ comandă OBD) și apelează `ScannerOBD.TrimiteComandaAsync("010C")`.
4. `ScannerOBD` scrie comanda pe portul serial către adaptorul ELM327, care interoghează magistrala CAN a vehiculului și primește răspunsul de la ECU.
5. Răspunsul brut (ex. `41 0C 0C F8`) este returnat către `CommandProcessor`.
6. `CommandProcessor` apelează `ScannerOBD.DecodeazaRaspuns("010C", RawHex)`. În cadrul acestei metode sunt extrași octeții A, B, C, D din șirul hexazecimal, după care se apelează `ObdTranslator.GetDecodedValue("0C", A, B, C, D)`, care aplică formula `(256·A + B) / 4 = 830 RPM`.
7. Rezultatul (`CommandResult`) este returnat la `Form1`, care îl afișează utilizatorului.
8. În paralel, operația este salvată asincron în baza de date cloud prin `SupabaseService.InsertLogAsync()`, fără a bloca firul de interfață.


<img width="2560" height="1200" alt="secventa_citire_rpm" src="https://github.com/user-attachments/assets/edd8e7f4-a0b2-421e-b646-d4a06bed7a1e" />

