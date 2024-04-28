using System.Collections;
using System.Collections.Generic;
using Ionic.Zip;
using Naninovel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class RenderAVGDisplay : MonoBehaviour
{
    public RawImage rawImage;
    RenderTexture renderTexture;

    public void setRenderTexture(RenderTexture input){
        renderTexture = input;
        rawImage.texture = renderTexture;
    }

    void Update(){
        // var naniCamera = Engine.GetService<ICameraManager>().Camera;
        // Camera camera = naniCamera.gameObject.GetComponent<Camera>();
        // int width = camera.pixelWidth;
        // int height = camera.pixelHeight;
        // renderTexture = new RenderTexture(width,height,24);
        // renderTexture = naniCamera.gameObject.GetComponent<Camera>().activeTexture;
        // // 将相机渲染到临时 RenderTexture
        // // naniCamera.gameObject.GetComponent<Camera>().targetTexture = renderTexture;
        // // naniCamera.gameObject.GetComponent<Camera>().Render();
        // rawImage.texture = renderTexture;
    }
}
