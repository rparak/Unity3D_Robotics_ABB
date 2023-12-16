# Unity3D Industrial Robotics: ABB IRB 120

<p align="center">
<img src="https://github.com/rparak/Unity3D_Robotics_ABB/blob/main/images/abb_background.png" width="800" height="500">
</p>

## Requirements:

**Software:**
```bash
ABB RobotStudio, Blender, Unity3D 2020.3.48f1, Visual Studio 2017/2019
```

**Supported on the following operating systems:**
```bash
Universal Windows Platform, Android
```

| Software/Package      | Link                                                                                  |
| --------------------- | ------------------------------------------------------------------------------------- |
| Blender               | https://www.blender.org/download/                                                     |
| Unity3D               | https://unity3d.com/get-unity/download/archive                                        |
| Unity HDRI Pack       | https://assetstore.unity.com/packages/2d/textures-materials/sky/unity-hdri-pack-72511 |
| ABB RobotStudio       | https://new.abb.com/products/robotics/robotstudio/downloads                           |
| Robot Web Services    | https://developercenter.robotstudio.com/api/rwsApi/                                   |
| Visual Studio         | https://visualstudio.microsoft.com/downloads/                                         |

## Project Description:

The project is focused on a simple demonstration of client-server communication via RWS (Robot Web Services), which is implemented in Unity3D. The project demonstrates the Digital-Twin of the ABB IRB 120 robot with some additional functions. The application uses performance optimization using multi-threaded programming.

This solution can be used to monitor a real robot or to simulate it (ABB RobotStudio in Windows). The Unity3D Digital-Twin application was tested on the IRB120 robot, both on real hardware and on simulation. The results of this example will be published on youtube.

Main functions of the ABB IRB 120 Digital-Twin model:
- Camera Control
- Connect/Disconnect -> Real HW or Simulation
- Read Data (Cartesian / Joint Position diagnostics)

The application can be installed on a mobile phone, tablet or computer, but for communication with the robot it is necessary to be in the same network.

The project was realized at the Institute of Automation and Computer Science, Brno University of Technology, Faculty of Mechanical Engineering (NETME Centre - Cybernetics and Robotics Division).

**Appendix:**

Example of a simple data processing application (Robot Web Services):

[ABB Robot - Data Processing](https://github.com/rparak/ABB_Robot_data_processing/)

<p align="center">
<img src="https://github.com/rparak/Unity3D_Robotics_ABB/blob/main/images/abb_1.PNG" width="800" height="500">
</p>

## Project Hierarchy:

**Repositary [/Unity3D_Robotics_ABB/ABB_Unity_App/Assets/]:**
```bash
[ UI + Main Control           ] /Script/UI/
[ Data Processing             ] /Script/ABB/
[ Individual objects (.blend) ] /Object/Blender/
[ Images (UI)                 ] /Image/
[ Scene of the Application    ] /Scenes/
```

<p align="center">
<img src="https://github.com/rparak/Unity3D_Robotics_ABB/blob/main/images/abb_h.PNG" width="800" height="500">
</p>

## Digital-Twin Application:

<p align="center">
<img src="https://github.com/rparak/Unity3D_Robotics_ABB/blob/main/images/abb_dt_1.png" width="800" height="500">
<img src="https://github.com/rparak/Unity3D_Robotics_ABB/blob/main/images/abb_dt_2.png" width="800" height="500">
<img src="https://github.com/rparak/Unity3D_Robotics_ABB/blob/main/images/abb_dt_3.png" width="800" height="500">
<img src="https://github.com/rparak/Unity3D_Robotics_ABB/blob/main/images/abb_rh_1.png" width="800" height="500">
</p>

## Result:

Youtube: https://www.youtube.com/watch?v=LVRx4pJCO2w

## Contact Info:
Roman.Parak@outlook.com

## Citation (BibTex)
```bash
@misc{RomanParak_Unity3D,
  author = {Roman Parak},
  title = {A digital-twins in the field of industrial robotics integrated into the unity3d development platform},
  year = {2020-2021},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/rparak/Unity3D_Robotics_Overview}}
}
```
## License
[MIT](https://choosealicense.com/licenses/mit/)

