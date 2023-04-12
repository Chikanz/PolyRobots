using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using GLTFast;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using PolyPizza;
using UnityEngine.Serialization;

namespace PolyPizza
{
    public class APIManager : MonoBehaviour
    {
        public static APIManager instance;

        private const float MIN_BOUNDING_BOX_SIZE_FOR_SIZE_FIT = 0.001f;

        const string URL = "https://api.poly.pizza/v1/";
        public string APIKEY;

        [FormerlySerializedAs("CacheToFile")] public bool CacheModelsToFile = true;

        public GameObject modelBlank;

        private Dictionary<string, dynamic> RequestCache = new Dictionary<string, dynamic>();

        private async void Awake()
        {
            if (!instance) instance = this;

            if (APIKEY == string.Empty)
            {
                Debug.LogError("API key wasn't set. Please get a key from https://poly.pizza/settings/api");
                enabled = false;
            }
        }

        private async void Start()
        {
        }

        //stolen from old poly API code lol
        static bool ComputeScaleFactorToFit(Bounds bounds, float desiredSize, out float scaleFactor)
        {
            float biggestSide = Math.Max(bounds.size.x, Math.Max(bounds.size.y, bounds.size.z));
            if (biggestSide < MIN_BOUNDING_BOX_SIZE_FOR_SIZE_FIT)
            {
                scaleFactor = 1.0f;
                return false;
            }

            scaleFactor = desiredSize / biggestSide;
            return true;
        }


        /// <summary>
        /// Take a model from the Poly Pizza API and load it into the scene
        /// </summary>
        public async UniTask<GameObject> MakeModel(Model model, float scale = 1, bool positionCenter = false)
        {
            var newObj = Instantiate(modelBlank);
            var gltf = newObj.GetComponent<GltfBoundsAsset>();

            var url = model.Download.ToString();
            if (CacheModelsToFile)
            {
                string savePath = string.Format("{0}/{1}.glb", Application.persistentDataPath, model.Id);

                if (!File.Exists(savePath))
                {
                    using (var client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(new Uri(url), savePath);
                    }
                }

                url = savePath;
            }

            try
            {
                await gltf.Load(url);
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading " + url);
                return null;
            }

            var box = newObj.GetComponent<BoxCollider>();

            if (ComputeScaleFactorToFit(gltf.bounds, scale, out float scaleFactor))
            {
                // newObj.transform.GetComponentInChildren<MeshRenderer>().transform.localScale = Vector3.one * scaleFactor;
                newObj.transform.localScale = Vector3.one * scaleFactor;
                // box.size *= scaleFactor;
                // box.center *= scaleFactor;
            }

            if (positionCenter)
            {
                Vector3 offset = newObj.transform.position - box.center;
                newObj.transform.position -= offset;
            }

            return newObj;
        }

        private UnityWebRequestAsyncOperation GetReq(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("X-Auth-Token", APIKEY);
            return www.SendWebRequest();
        }

        /// <summary>
        /// Given a model ID, get the model from the Poly Pizza API
        /// 
        public async UniTask<Model> GetModelByID(string id)
        {
            var res = await GetReq($"{URL}/model/{id}");

            if (res.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(res.downloadHandler.text);
                return null;
            }
            else
            {
                return Model.FromJson(res.downloadHandler.text);
            }
        }

        /// <summary>
        /// Search the Poly Pizza API for a model matching the search query
        /// </summary>
        /// <param name="keyword">The model you want to search for, eg Apple, Monkey, Gun etc </param>
        /// <returns></returns>
        public async UniTask<Model> GetExactModel(string keyword)
        {
            if (keyword.Length <= 2) return null;

            var res = await GetReq($"{URL}/search/{keyword}");
            if (res.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(res.downloadHandler.text);
                return null;
            }
            else
            {
                var search = SearchResults.FromJson(res.downloadHandler.text);
                if (search.Total == 0) return null;

                var regex = new Regex($"(s|{keyword})", RegexOptions.IgnoreCase);
                foreach (var model in search.Results)
                {
                    // if (String.Equals(model.Title, keyword, StringComparison.CurrentCultureIgnoreCase)) return model;
                    if (regex.Match(model.Title).Success)
                        return model;
                }

                return search.Results[0];
            }
        }

        /// <summary>
        /// Get the top models of all time from the Poly Pizza API
        /// </summary>
        /// <param name="limit">How many models to get. I think this caps at like 500 or something</param>
        /// <returns>Model object array</returns>
        public async UniTask<Model[]> GetPopular(int limit)
        {
            int pagesToGet = Mathf.CeilToInt(limit / 32.0f);
            print("getting " + pagesToGet + " pages for " + limit);
            List<Model> models = new List<Model>();
            for (int i = 0; i < pagesToGet; i++)
            {
                var res = await getURL($"{URL}/search?limit=32&page={i}");
                if (res != null)
                {
                    var search = SearchResults.FromJson(res);
                    models.AddRange(search.Results);
                    print("Got page " + i);
                }
            }

            return models.ToArray();
        }

        private async UniTask<string> getURL(string url)
        {
            //file cache
            string filename = WebUtility.UrlEncode(url);
            string reqPath = string.Format("{0}/{1}", Application.persistentDataPath, filename);
            if (File.Exists(reqPath))
            {
                return File.ReadAllText(reqPath);
            }
                
            //Hit server
            var res = await GetReq(url);
            if (res.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(res.downloadHandler.text);
                return null;
            }
            else
            {
                File.WriteAllText(reqPath, res.downloadHandler.text);
                return res.downloadHandler.text;
            }
        }

        /// <summary>
        /// API returns the orbit of the camera, so we convert it to a rotation using
        /// https://stackoverflow.com/questions/48841969/theta-phi-to-quaternion-rotation-unity3d-c-sharp
        /// and then ignoring the x component and inverting the y component since the orbit is for the camera not model
        /// </summary>
        public static Quaternion OrbitToRotation(Orbit orbit)
        {
            return Quaternion.Euler(0, (Mathf.Rad2Deg * orbit.theta), 0f);
        }
    }
}