// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;
using UnityEngine.Rendering;


namespace AmazingAssets.CurvedWorld.Examples
{
    public class ManualControllerUpdate : MonoBehaviour
    {        
        public CurvedWorldController curvedWorldController;


        void Start()
        {
            //Check if using Scriptable render pipeline
            if(GraphicsSettings.defaultRenderPipeline != null)
                RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        }


        //OnPreRender - is used only with built-in render pipeline
        private void OnPreRender()
        {
            if (curvedWorldController != null)
                curvedWorldController.ManualUpdate();
        }


        //BeginCameraRendering - is used only with Scriptable render pipeline, as there is no 'OnPreRender' method to use.
        void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            ManualControllerUpdate thisScript = camera.GetComponent<ManualControllerUpdate>();


            //Befor rendering THIS camera, force Curved World Controller to update shader data based on its properties.
            if (thisScript != null && thisScript.curvedWorldController != null)
            {
                thisScript.curvedWorldController.ManualUpdate();
            }
        }
    }
}
