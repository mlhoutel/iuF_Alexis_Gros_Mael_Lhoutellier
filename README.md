# iuF Project - School project
Reading of datas from a 3D Depth Camera ([Intel® RealSense™ D435](https://www.intelrealsense.com/depth-camera-d435/)) through the [Intel Realsense Api](https://github.com/IntelRealSense/librealsense):

## First Step : Extracting of the Datas from the Video Stream

### Reader Class
#### FromFile: 
Read the file from the path and get the Pipeline.

#### FromDevice
Detect the camera plugged and get the Pipeline.

### Streamer Class
#### Pixels
Extraction of the Image Pixels with the color and depth in meter.
```
pixel 254 / 307200:      position [254,0,2601],  color [124,125,119]
```
#### Point Cloud
Extraction of the Points with the color anb position in meter.
```
vertice 160 / 200:       position [-0,9332999,-1,3640885,3,456],  color [24,20,13]
```

#### Debug Displays in Ascii
**Example: ASCII Depth**
```
Duration time: 00:00:09.77182 56
Camera name: Intel RealSense D435
Firmware version: 05.12.07.100

                             hBXBB.
                            nWWWWWW.
                           hWWWWWWW:
                            :WWWWWW.
                             nWWWWX
                             BWWWWWh
                          .hWWWWWWWWWBn
                        nXWWWWWWWWWWWWWWB.
                       :WWWWWWWWWWWWWWWWWW.
                      .WWWWWWWWWWWWWWWWWWWh
                      XWWWWWWWWWWWWWWWWWWWh
                     BWWWWWWWWWWWWWWWWWWWW
                    BWWWWWWWWWWWWWWWWWWWWW
                   BWWWWX XWWWWWWWWWWWWWWW.
                  .WWWW:  XWWWWWWWWWWWWWWh
                 .XWWW:   WWWWWWWWWWWWWWX.
                 XWWWXhBBWWWWWWWWWWWWWWWh
                nWWW:   :BWWWWWWWWWWWWWWX
                WWW.      WWWWWWWWWWWWWWB
                hh.      .WWWWWWWWWWWWWWn
                          XWWWWWWWWWWWWW.
                          BWWWWWXnWWWWWX
                          nWWWWW. WWWWWW
```

## Next Step : Export to the Display device

Use the [Nuitrack SDK](https://github.com/3DiVi/nuitrack-sdk) to generate the skeleton, then export the datas and display it with Unity.
Create an augmented reality scene where the user can interact and see himself, using a virtual reality headset

