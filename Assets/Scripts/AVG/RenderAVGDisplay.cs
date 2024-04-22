using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderAVGDisplay : MonoBehaviour
{
    public RenderTexture renderTexture;
    public RawImage rawImage;

    void Start()
    {
        // 将RawImage的纹理设置为RenderTexture
        rawImage.texture = renderTexture;
    }
}
