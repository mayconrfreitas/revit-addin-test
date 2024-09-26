# Revit Addin Test

## Task 1: Room Data Extraction

Create a Revit add-in using **Revit API** that extracts room data from a sample Revit project (check attachment). The script should:

1. Extract room data, such as room name, number, area, and volume

2. Calculate the volume occupied by family elements such as furniture, fixtures, or equipment within each room (Exclude door/windows/voids).

3. Determine the space utilization ratio by comparing the occupied volume to the total volume of each room.

4. Categorize rooms based on their space utilization ratios using the following thresholds:

	- **Under-utilized**: Space utilization ratio less than `0.3 (30%)`
	- **Well-utilized**: Space utilization ratio between `0.3 (30%)` and `0.8 (80%)`
	- **Over-utilized**: Space utilization ratio greater than `0.8 (80%)`

5. Generate a report, either in CSV or Excel format, that lists all rooms, their total area and volume, occupied volume, space utilization ratios, and utilization categorization.

## Task 2: Import OBJ Geometry into Revit

Create a Revit add-in that imports a simple OBJ file (check attachment) containing primitive objects into a Revit project as native Revit geometry. The script should:

1. Read the provided OBJ file and parse the geometry information (vertices, faces, etc.).

2. Convert the parsed OBJ geometry data into native Revit geometry.

3. Create Revit family instances or model in-place components based on the imported geometry.

4. Place the imported geometry instances within the Revit project, maintaining their original positions and orientations from the OBJ file.

> [!NOTE]
> Do not use any intermediary library to parse the data.
