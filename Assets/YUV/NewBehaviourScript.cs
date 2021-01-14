/**
*所属公司: #COMPANY#
*作者: #AUTHOR#
*版本: #VERSION#
*Unity版本：#UNITYVERSION#
*创建日期: #DATE#
*描述:
*历史修改记录:
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Renderer target;

    int videoW = 640;
    int videoH = 480;

    byte[] bufY = null;
    byte[] bufU = null;
    byte[] bufV = null;
    byte[] bufUV = null;

    Texture2D texY = null;
    Texture2D texU = null;
    Texture2D texV = null;
    Texture2D texUV = null;

    void Start()
    {
        texY = new Texture2D(videoW, videoH, TextureFormat.Alpha8, false);
        //U分量和V分量分别存放在两张贴图中
        texU = new Texture2D(videoW >> 1, videoH >> 1, TextureFormat.Alpha8, false);
        texV = new Texture2D(videoW >> 1, videoH >> 1, TextureFormat.Alpha8, false);

        texUV = new Texture2D(videoW >> 1, videoH >> 1, TextureFormat.RGBA4444,false);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Load YUV"))
        {
            LoadYUV();

            texY.LoadRawTextureData(bufY);
            texU.LoadRawTextureData(bufU);
            texV.LoadRawTextureData(bufV);

            texY.Apply();
            texU.Apply();
            texV.Apply();
            texUV.LoadRawTextureData(bufUV);
            texUV.Apply();
        }

        if (GUILayout.Button("Render YUV"))
        {
            target.sharedMaterial.SetTexture("_MainTex", texY);
            target.sharedMaterial.SetTexture("_UTex", texU);
            target.sharedMaterial.SetTexture("_VTex", texV);
            target.sharedMaterial.SetTexture("_UVTex", texUV);
        }
    }

    void LoadYUV()
    {
        string filePath = Application.dataPath + "/YUV/Resources/123.yuv";
        using (FileStream fstream = new FileStream(filePath, FileMode.Open))
        {
            try
            {
                byte[] buff = new byte[fstream.Length];
                fstream.Read(buff, 0, buff.Length);

                int firstFrameEndIndex = (int)(videoH * videoW * 1.5f);

                int yIndex = firstFrameEndIndex * 4 / 6;
                int uIndex = firstFrameEndIndex * 5 / 6;
                int vIndex = firstFrameEndIndex;

                bufY = new byte[videoW * videoH];
                bufU = new byte[videoW * videoH >> 2];
                bufV = new byte[videoW * videoH >> 2];
                bufUV = new byte[videoW * videoH >> 1];

                bool isSingle = false;
                int indexV = 0;
                int indexU = 0;

                for (int i = 0; i < firstFrameEndIndex; i++)
                {
                    
                    if (i < yIndex)
                    {
                        bufY[i] = buff[i];
                        continue;
                    }

                    if (isSingle)
                    {
                        bufU[indexV] = buff[i];
                        indexV++;
                        isSingle = false;
                    }
                    else
                    {
                        bufV[indexU] = buff[i];
                        indexU++;
                        isSingle = true;
                    }

                }

                //如果是把UV分量一起写入到一张RGBA4444的纹理中时，byte[]
                //里的字节顺序应该是  UVUVUVUV....
                //这样在shader中纹理采样的结果 U 分量就存在r、g通道。
                //V 分量就存在b、a通道。

                for (int i = 0; i < bufUV.Length; i += 2)
                {
                    bufUV[i] = bufU[i >> 1];
                    bufUV[i + 1] = bufV[i >> 1];
                }

                //如果不反转数组，得到的图像就是上下颠倒的
                //建议不在这里反转，因为反转数组还是挺耗性能的，
                //应该到shader中去反转一下uv坐标即可
                //Array.Reverse(bufY);
                //Array.Reverse(bufU);
                //Array.Reverse(bufV);
                //Array.Reverse(bufUV);

            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}
