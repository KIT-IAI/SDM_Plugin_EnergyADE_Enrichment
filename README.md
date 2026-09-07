# EnergyADE Enrichment
The EnergyADE Enrichment is a plugin for the [KITModelViewer](https://github.com/KIT-IAI/SDM_KITModelViewer) for enrichment of CityGML building models with building physics and building usage parameters. The EnergyADE extension is used to store these additional parameters in a standardized manner.

Regardless of whether a building or a district is being processed, the module primarily attempts to interpret the GML standard attributes function and yearOfConstruction in order to perform automatic enrichment. The following requirements apply to automatic enrichment:

-	The assignment of usage profiles relies on the ALKIS building code provided in the attribute function.
-	The assignment of the material definitions results from the attribute yearOfConstruction and is obtained from the TABULA data, using the [NaiS database](https://github.com/KIT-IAI/SDM_NaiS-DB).

Based on the parameters provided by the buildings, the enrichment process automatically assigns usage profiles and the material information. If this automatic assignment is not possible for one or more buildings, manual assignment to a corresponding ALKIS code or a corresponding building age class is also possible. 

<img width="1260" height="1370" alt="ADE_Enrichment_1" src="https://github.com/user-attachments/assets/f50060fc-49b0-4f4b-a5e9-26c19db95f12" />
<img width="1260" height="1370" alt="ADE_Enrichment_2" src="https://github.com/user-attachments/assets/87aa98ab-2449-4307-bc42-1760eab315a4" />
<img width="1260" height="1370" alt="ADE_Enrichment_3" src="https://github.com/user-attachments/assets/b18ff0c5-bac3-42cf-a55c-0184ea7ee45b" />


## User Interface
The user interface is based on WPF (Windows Presentation Foundation) and is developed in C# using the .NET Framework 4.8.

## Dependencies

### Use of vcpkg:

|Package Name         |Install Command                            |
|:---                 |:---                                       |
|curl                 |vcpkg install curl triplet=x64-windows     |
|fmt                  |vcpkg install fmt triplet=x64-windows      |
|geographiclib        |vcpkg install geographiclib triplet=x64-windows |
|libtess2             |vcpkg install libtess2 triplet=x64-windows |

## How to cite

```bibtex
@software{SDM_Plugin_EnergyADE_Enrichment,
	title        = {{SDM\_Plugin\_EnergyADE\_Enrichment}},
	author       = {Andreas Geiger},
	url          = {https://github.com/KIT-IAI/SDM_Plugin_EnergyADE_Enrichment},
	year         = {2024}
}
```

```bibtex
@inproceedings{Geiger.2024,
   author       = {Geiger, Andreas; Häfele, Karl-Heinz; Hagenmeyer, Veit},
   title        = {CityGML Data Preparation for Thermal Building Simulation on District Level},
   booktitle     = {Proceedings of BauSim Conference 2024: 10th Conference of IBPSA-DACH},
   year          = {2024},
   editor        = {Bednar, Thomas and Sint, Sabine},
   pages         = {[104--111]},
   address       = {Wien, Österreich},
   month         = {September},
   publisher     = {TU Wien},
   doi           = {10.26868/29761662.2024.14},
   isbn          = {978-3-200-10068-8}
}
```






