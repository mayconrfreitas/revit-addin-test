# Revit Addin Test

Code developed for the Revit Addin test, proposed by Snaptrude.

## Summary

- [Installation](#installation)
- [Challenges](#challenges)
	- [Task 1: Room Data Extraction](#task-1-room-data-extraction)
	- [Task 2: Import OBJ Geometry into Revit](#task-2-import-obj-geometry-into-revit)
- [Solution](#solution)
	- [Addin Structure](#addin-structure)
	- [My Approaches to Solving the Problems](#my-approaches-to-solving-the-problems)
		- [Task 1: Room Data Extraction](#task-1-room-data-extraction)
		- [Task 2: Import OBJ Geometry into Revit](#task-2-import-obj-geometry-into-revit)


## Installation

1. Download the `zip` file from the [Releases](https://github.com/mayconrfreitas/revit-addin-test/releases/latest) page.

1. Extract the contents of the `zip` file to `%appdata%\Autodesk\Revit\Addins\2024`.


## Challenges

### Task 1: Room Data Extraction

Create a Revit add-in using **Revit API** that extracts room data from a sample Revit project (check attachment). The script should:

1. Extract room data, such as room name, number, area, and volume

2. Calculate the volume occupied by family elements such as furniture, fixtures, or equipment within each room (Exclude door/windows/voids).

3. Determine the space utilization ratio by comparing the occupied volume to the total volume of each room.

4. Categorize rooms based on their space utilization ratios using the following thresholds:

	- **Under-utilized**: Space utilization ratio less than `0.3 (30%)`
	- **Well-utilized**: Space utilization ratio between `0.3 (30%)` and `0.8 (80%)`
	- **Over-utilized**: Space utilization ratio greater than `0.8 (80%)`

5. Generate a report, either in CSV or Excel format, that lists all rooms, their total area and volume, occupied volume, space utilization ratios, and utilization categorization.


🚀 [Go to Solution](#task-1-room-data-extraction-1) 🚀

### Task 2: Import OBJ Geometry into Revit

Create a Revit add-in that imports a simple OBJ file (check attachment) containing primitive objects into a Revit project as native Revit geometry. The script should:

1. Read the provided OBJ file and parse the geometry information (vertices, faces, etc.).

2. Convert the parsed OBJ geometry data into native Revit geometry.

3. Create Revit family instances or model in-place components based on the imported geometry.

4. Place the imported geometry instances within the Revit project, maintaining their original positions and orientations from the OBJ file.

> [!CAUTION]   
> Do not use any intermediary library to parse the data.

🚀 [Go to Solution](#task-2-import-obj-geometry-into-revit-1) 🚀

## Solution

I started by asking about the purpose of the test, whether it would be to evaluate only the tasks themselves or also the creation of the addin, to understand if I could use a template or if I had to create it from scratch, and which version of Revit I should focus on. I was informed that I should create the addin from scratch and in any version I preferred.

I chose to create the addin for **Revit 2024**.

> [!IMPORTANT]  
> The addin was developed using the Revit API for Revit 2024.

First, I asked ChatGPT to help me configure the folder structure of my addin and plan it, following the [MVVM pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel), to keep the code well-organized and as decoupled as possible.

The addin was divided into the following structure:

### Addin Structure

[📦RevitAddinText](./RevitAddinTest/) `Solution root folder`  
 ┣ [📂AddinManifest](./RevitAddinTest/AddinManifest/) `Folder with Addin Manifest`   
 ┃ ┗ [📜RevitAddinTest.addin](./RevitAddinTest/AddinManifest/RevitAddinTest.addin) `Addin Manifest`  
 ┣ [📂Application](./RevitAddinTest/Application/) `Application-related files`  
 ┃ ┣ [📜App.xaml](./RevitAddinTest/Application/App.xaml) `WPF application configuration of the project`  
 ┃ ┣ [📜App.xaml.cs](./RevitAddinTest/Application/App.xaml.cs) `WPF application configuration of the project code`  
 ┃ ┗ [📜RevitApp.cs](./RevitAddinTest/Application/RevitApp.cs) `Revit application (IExternalApplication)`  
 ┣ [📂Commands](./RevitAddinTest/Commands/) `Revit (IExternalCommand) and WPF (ICommand)-related files`  
 ┃ ┣ [📜ImportObjGeometryCommand.cs](./RevitAddinTest/Commands/ImportObjGeometryCommand.cs) `Revit command to implement Task 2`  
 ┃ ┣ [📜RelayCommand.cs](./RevitAddinTest/Commands/RelayCommand.cs) `WPF ICommand generic implementation to be reused`  
 ┃ ┗ [📜RoomDataExtractionCommand.cs](./RevitAddinTest/Commands/RoomDataExtractionCommand.cs) `Revit command to implement Task 1`   
 ┣ [📂Data](./RevitAddinTest/Data/) `Folder to store the sample files`  
 ┃ ┣ [📜Snaptrude.rvt](./RevitAddinTest/Data/Snaptrude.rvt) `Sample Revit model`  
 ┃ ┗ [📜snaptrude.obj](./RevitAddinTest/Data/snaptrude.obj) `Sample OBJ file`  
 ┣ [📂Helpers](./RevitAddinTest/Helpers/) `Folder to store helper classes`  
 ┃ ┣ [📜FileHelper.cs](./RevitAddinTest/Helpers/FileHelper.cs) `Help to get Open/Save file paths`  
 ┃ ┣ [📜GeometryHelper.cs](./RevitAddinTest/Helpers/GeometryHelper.cs) `Help to calculate Occupied Volume, Check if geometry is void, Parse OBJ files, and create Revit Geometry`  
 ┃ ┣ [📜RevitAPIHelper.cs](./RevitAddinTest/Helpers/RevitAPIHelper.cs) `Help to check Volume Calculation Settings, handle Zoom to Elements and Get Elements Inside a Room`   
 ┃ ┗ [📜WindowHelper.cs](./RevitAddinTest/Helpers/WindowHelper.cs) `Help to make Revit Owner of the WPF windows`  
 ┣ [📂Models](./RevitAddinTest/Models/) `Folder to store model classes`  
 ┃ ┣ [📜OBJGeometryModel.cs](./RevitAddinTest/Models/OBJGeometryModel.cs) `OBJ Geometry model class`  
 ┃ ┗ [📜RoomModel.cs](./RevitAddinTest/Models/RoomModel.cs) `Room model class`   
 ┣ [📂Properties](./RevitAddinTest/Properties/)   
 ┃ ┗ [📜AssemblyInfo.cs](./RevitAddinTest/Properties/AssemblyInfo.cs)   
 ┣ [📂Resources](./RevitAddinTest/Resources/) `Folder to store addin Icons`   
 ┃ ┗ [📂Icons](./RevitAddinTest/Resources/Icons/)  
 ┃   ┣ [📜import-obj-geometry-16.png](./RevitAddinTest/Resources/Icons/import-obj-geometry-16.png)  
 ┃   ┣ [📜import-obj-geometry-24.png](./RevitAddinTest/Resources/Icons/import-obj-geometry-24.png)  
 ┃   ┣ [📜room-data-extraction-16.png](./RevitAddinTest/Resources/Icons/room-data-extraction-16.png)  
 ┃   ┗ [📜room-data-extraction-24.png](./RevitAddinTest/Resources/Icons/room-data-extraction-24.png)  
 ┣ [📂Services](./RevitAddinTest/Services/) `Folder to store Services classes - classes responsible for the business logic` 
 ┃ ┣ [📜ImportObjGeometryService.cs](./RevitAddinTest/Services/ImportObjGeometryService.cs) `Import OBJ logic`  
 ┃ ┣ [📜ReportService.cs](./RevitAddinTest/Services/ReportService.cs) `Export CSV file logic`  
 ┃ ┗ [📜RoomDataExtractionService.cs](./RevitAddinTest/Services/RoomDataExtractionService.cs) `Extract Room data logic`   
 ┣ [📂ViewModels](./RevitAddinTest/ViewModels/) `Folder to store ViewModels`  
 ┃ ┣ [📜BaseViewModel.cs](./RevitAddinTest/ViewModels/BaseViewModel.cs) `Base class with the implementation of INotifyPropertyChanged to be inherited`  
 ┃ ┣ [📜ImportObjGeometryViewModel.cs](./RevitAddinTest/ViewModels/ImportObjGeometryViewModel.cs) `Connects the ImportObjGeometry View, model and logic`   
 ┃ ┗ [📜RoomDataExtractionViewModel.cs](./RevitAddinTest/ViewModels/RoomDataExtractionViewModel.cs) `Connecys the RoomDataExtraction View, model and logic`   
 ┣ [📂Views](./RevitAddinTest/Views/) `Folder to store the UIs`  
 ┃ ┣ [📜ImportObjGeometryView.xaml](./RevitAddinTest/Views/ImportObjGeometryView.xaml) `ImportObjGeometry View (UI)`  
 ┃ ┣ [📜ImportObjGeometryView.xaml.cs](./RevitAddinTest/Views/ImportObjGeometryView.xaml.cs) `ImportObjGeometry View (UI) code`  
 ┃ ┣ [📜RoomDataExtractionView.xaml](./RevitAddinTest/Views/RoomDataExtractionView.xaml) `RoomDataExtraction View (UI)`  
 ┃ ┗ [📜RoomDataExtractionView.xaml.cs](./RevitAddinTest/Views/RoomDataExtractionView.xaml.cs) `RoomDataExtraction View (UI) code`   
 ┣ [📜.gitignore](./RevitAddinTest/.gitignore)  
 ┣ [📜RevitAddinTest.csproj](./RevitAddinTest/RevitAddinTest.csproj) `Visual Studio Project File`  
 ┗ [📜RevitAddinTest.sln](./RevitAddinTest/RevitAddinTest.sln) `Visual Studio Solution File`  

Still regarding the overall structure of the plugin, I chose to use WPF commands whenever possible to avoid Revit crashes and to develop a solution aligned with the chosen pattern. So I asked ChatGPT to help me create the `RelayCommand.cs` file, which contains a generic implementation of WPF's `ICommand`, so I could use it for all the commands in the plugin.

Additionally, I separated the plugin logic into `Services` to keep the `ViewModel` and `Commands` cleaner and easier to maintain. I also created `Helper` files for specific and repetitive code.

For the `ViewModels`, I created a base class with the implementation of `INotifyPropertyChanged`, so that other classes could inherit from it and avoid repeating code.

### My Approaches to Solving the Problems

> [!IMPORTANT]  
> For more details and additional information, please refer to the comments in the code!

#### Task 1: Room Data Extraction

For [Task 1](#task-1-room-data-extraction), Room Data Extraction, I planned to collect all the Rooms from the project, gather the requested information for each room, and perform the necessary calculations to determine space usage. Afterward, I would display the data in a table format, with the option to export it to a CSV file at the end.

Since the prompt did not specify the units of measurement, I chose to display the values in square meters and cubic meters for easier verification. However, to remove this conversion, I could simply use the values in Revit's internal units.

When collecting the Rooms and starting to extract the information, I encountered some issues:

1. I noticed that the Rooms in the model all had a Volume of 0 (zero). I researched and found that it was necessary to enable the `ComputeVolumes` property in the `AreaVolumeSettings` class. So I added a check at the beginning of the command to ensure that the property is enabled.

1. Even after enabling the property, some Rooms still had a `Volume` of 0. Upon evaluating the model, I realized that these Rooms were not placed, so I considered only Rooms that were placed or had a `Volume` greater than 0.

1. I also noticed that many Rooms, if not all, had the `Upper Limit` (level) set to the same level as the current `Level`, and the `Limit Offset` was greater than the ceiling height. In a real-world scenario, I would implement a warning to the user about this, as it appears to be a modeling error, or I would automatically fix the issue. However, for this test, I assumed the model was correct.

1. To configure the filters for the categories of elements to be collected, ideally, I would create another command for this configuration, which would include a UI for the user to select the categories they want to collect. However, for this test, I created a fixed filter to disregard the categories mentioned in the prompt.

	- One of the problems with this approach is that I would need to create workarounds for each specificity that appears. For instance, in the Garage, the car model was hidden in the views, but the car's volume was still being counted. To solve this, I would need to create a filter to disregard the car category or a rule to consider only elements visible in 3D.
	
	- I did not find an ideal way to check if a solid is a void. I found two ways to do this: one is to check if the solid's volume is equal to zero, and the other is to check if the solid's `GraphicsStyleId` is equal to `ElementId(BuiltInCategory.OST_IOSCuttingGeometry)`, but I did not notice any difference in my tests. For more details, visit the [GeometryHelper.cs](./RevitAddinTest/Helpers/GeometryHelper.cs) file.

1. I chose to use the `BoundingBox` of the Rooms to create a `BoundingBoxIntersectsFilter` to collect elements inside the Room using the `FilteredElementCollector` with the `WherePasses()` method. However, I realized that the filter was not working as expected, as it was returning additional elements, such as sockets and switches, where only a small part of their geometry was inside the room but most of it was within the wall. I then thought of using `BoundingBoxIsInsideFilter`, but some Rooms that contained elements simply returned empty lists. I believe it has to do with the insertion point or the host of the families. So, I decided to go back to `BoundingBoxIntersectsFilter` and add a tolerance variable to reduce the `BoundingBox` limits of the Room and not collect elements that were in the walls, floors, or ceilings.

1. To get the volumes of the families, I initially thought of using the built-in parameter `HOST_VOLUME_COMPUTED`, but I quickly noticed discrepancies when compared to the sum of the family's solid volumes. So, I opted to get the volumes of the family's solids and sum them up.

Finally, after collecting all the information, I added a **Total** line at the end, created a UI to display the data in a table format before exporting, and added a button to export to a CSV file, opening a Save Dialog for the user to choose the file location and name.

> [!NOTE]  
> I added a Double Click functionality to the table rows so that the addin zooms in on the selected Room.


#### Task 2: Import OBJ Geometry into Revit

For [Task 2](#task-2-import-obj-geometry-into-revit), Import OBJ Geometry into Revit, I didn't know how to start, as I had never worked with converting an [OBJ file](https://en.wikipedia.org/wiki/Wavefront_.obj_file) into Revit geometry. So I asked ChatGPT to help me understand how the OBJ file worked, and I also researched the Wavefront documentation and other sites to understand the file structure.

I created the model [OBJGeometryModel](./RevitAddinTest/Models/OBJGeometryModel.cs) to store the information from the OBJ file.

I also created a UI for the user to select the OBJ file they want to import, along with a button to perform the import.

I realized that, basically, I needed to read the lines of the file and focus on those starting with `v` (vertices), `f` (faces), and `o` (objects).

So, I used `StreamReader` to read the file and used [Regex](https://en.wikipedia.org/wiki/Regular_expression) to identify lines that started with `v`, `f`, and `o`.

1. When the line starts with `o`, it means this is a new object. I create a new `OBJGeometryModel`, name it with the value from the line after `o`, and, if the previous one had data, I add it to the list of geometries.

1. When the line starts with `v`, it indicates a vertex. I take the `x`, `y`, and `z` values and create a Revit `XYZ` object to add to the vertex list.

	- I learned that the coordinate system of OBJ is different from Revit: the Y-axis points up in OBJ, while in Revit, the Z-axis points up. So, to convert, I swapped the `y` value to `z`.
	
	- Additionally, the OBJ vertex has 4 values, with the 4th value (`w`) being optional, so I only take the first 3 values.

1. When the line starts with `f`, it represents a face. Considering the OBJ face structure as `f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3 ...`, where `v1`, `v2`, `v3`, ... `vn` are vertex indices and `vt` and `vn` are texture and normal indices, respectively, I only take the vertex indices, as there was nothing in the challenge prompt about textures.

	- According to the documentation, the vertex index in OBJ starts at 1, whereas in arrays it starts at 0, so I subtracted 1 from the index.

	- **HOWEVER**, the issue that took me the most time to understand and solve was that the `Icosphere` element of the provided [sample OBJ](./RevitAddinTest/Data/snaptrude.obj) was not being created due to an out-of-range index error; only the cube was being created. I loaded the OBJ in an online viewer and saw that the `Icosphere` was being rendered correctly. After much trial and error, I realized that the smallest vertex index of the `Icosphere` was not 1 but 9, so instead of subtracting 1 from the index, I subtracted 9. And voilà, the `Icosphere` was created correctly!

> [!IMPORTANT]  
> Another important point noted in the documentation is that OBJ does not contain unit information. Therefore, it would be necessary to add an option in the UI for the user to choose the desired measurement unit for the import or a scale factor to correctly scale the imported geometry. However, for this test, I disregarded this step.

To convert the processed OBJ data into a geometry that could be inserted as a family in Revit, I used `DirectShape` to create an instance of an in-place model family and `TessellatedShapeBuilder` to generate a list of `GeometryObject` that would define the shape of these families (with the help of ChatGPT).

When the OBJ is converted, and the family is created, the addin zooms in on the created family and displays a success message.