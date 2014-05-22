Hermes
======

(cz) Popis funkčnosti
---------------------

(Základní popis frameworku Hermés)
Projekt Hermés je framework pro snadné vytváření, testování a exekuování
strategií a portfolií pro obchodování na finančních trzích.

(Cíle projektu)

(Popis základních stavebních prvků)
Architektura je navržena tak, aby maximálne odrážela skutečnou situaci na
finančních trzích. 

```
  +--------+
  | Broker |
  +--------+
       |
       |
 +-----------+            +-----------+
 | Portfolio |------+-----| Strategie |
 +-----------+      |     +-----------+
                    |
                    |     +-----------+
                    +-----| Strategie |
                    |     +-----------+
                   ...        ....
                    |     +-----------+
                    +-----| Strategie |
                          +-----------+
						  
```

(Příklad užití)

(Licence)
V tuto chvíli je projekt vytvářen pro soukromé účely a není dovoleno
jakkoliv upravovat, šířit ani prodávat části zdrojového kódu, jeho binární
podoby ani další software obsahující framework Hermés ani jeho části.

(cz) Popis architektury
-----------------------

Framework Hermés je event-driven. Dispatching událostí řídí třída Kernel, která
je automaticky vytvořená spoječně s vytvořením třídy Portfolio.

Idea je, že uživatel zkonstruuje jednotlivé třídy a pospojuje je do jednoho
logického celku. Poté zavolá void Portfolio::Initialize() a portfolio
zinitializuje všechny součásti do stavu přepraveného ke spuštění.

TODO: Kde se zavolá Run a jak se řekne, že je konec?

### Portfolio ###
Portfolio je vstupním bodem ke každé simulaci. Přijímá signály od jednotlivých
strategií a na jejich základě se rozhoduje které příkazy skutečně exekuuje.
Portfolio je zodpovědné za position sizing a risk management.

### Strategy ###
Strategie generuje na základě vstupních dat asociovaného DataFeedu signály typu
SignalEvent. Konkrétně Buy, Hold a Sell. Pozn: Portfolio se může rozhodnout
dané příkazy neexekuovat.

### Broker ###
Simuluje chování reálného brokera. Je zodpovědný za zahrnutí popratků, marginů,
slippage, spread a dalších vlivů, které se mohou vyskytnout při reálném
obchodování.

### DataFeed ###
Třída zodpovědná za poskytování dat z finančních trhů do celé simulace.
Musí podporovat náhled o N kroků zpět.

### Kernel ###
Je třída, která se stará o řízení dispatchingu celé simulace.
