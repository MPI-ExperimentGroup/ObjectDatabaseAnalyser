# Object Database Analyser

This editor tool allows you to generate a csv file describing the details of a 3d object collection. This allows anyone to quickly evaluate the quality and specifics of a 3d object collection by looking at tricount, number of materials used, physical size, naming etc. Additionally, it contains tools to batch extract textures and materials and create thumbnails of each object in the collection.

This tool was created at the [MPI](https://www.mpi.nl/) to quickly get a qualitive overview of the many (sometimes external) object databases we are working with.

Developed in Unity 2021.3.33f1

Open it from the Unity Editor toolbar -> Object Database Utilities -> Object Database Utilities..

![WindowCapture_2024-03-05_11-46-03](https://github.com/MPI-ExperimentGroup/ObjectDatabaseAnalyser/assets/160507576/03448372-f04a-47df-8d62-3bc793448e00)

CSV file imported in Excel

![ObjectDatabaseAnalyzerExcel2](https://github.com/MPI-ExperimentGroup/ObjectDatabaseAnalyser/assets/160507576/893756b6-116e-47f8-9a3d-33f2a70a4917)

Generated thumbnails from various user defined angles

![WindowCapture_2024-03-05_12-45-32](https://github.com/MPI-ExperimentGroup/ObjectDatabaseAnalyser/assets/160507576/10a7dcf6-d6cf-4787-8b64-24e38683f1f4)

## Youtube video quick demo:

[![Youtube video quick demo](https://img.youtube.com/vi/rGhByNUoUWg/0.jpg)](https://www.youtube.com/watch?v=rGhByNUoUWg)

## Features

- Generate detailed csv overview file
- Regex filter (for example '.*\.fbx' to only process fbx files) 
- Batch extract textures
- Batch extract materials
- create thumbnails with user defined camera angles and resolution

## Installation

- Clone from this repo
- download and import Unity Package : https://github.com/MPI-ExperimentGroup/ObjectDatabaseAnalyser/releases/tag/v1.0.0 

## Credits

Unity Runtime Preview Generator
https://github.com/yasirkula/UnityRuntimePreviewGenerator
