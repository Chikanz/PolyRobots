using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using dook.tools;
using PolyPizza;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int width;
    public int height;
    public float spacing;

    public GameObject platform;

    private List<Transform> platforms = new List<Transform>();
    private Transform[,] plane;
    private float platformObjWidth;
    [SerializeField] private float spawnerDelaySpacing = 1;
    private float platformObjHeight;
    private int _nextPlat;

    public string[] Exclude;

    [Serializable]
    public class RotTuple : SerializableDictionary<string, float>
    {
    };

    public RotTuple rotMap = new RotTuple
    {
        {"1231331", 9}
    };

    // Start is called before the first frame update
    void Awake()
    {
        plane = new Transform[width, height];
        platformObjWidth = platform.GetComponent<BoxCollider>().size.x;
        platformObjHeight = platform.GetComponent<BoxCollider>().size.y;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var p = Instantiate(platform,
                    transform.position + new Vector3((((platformObjWidth + spacing) * i)), 0,
                        (((platformObjWidth + spacing) * j))),
                    Quaternion.identity);
                platforms.Add(p.transform);
                plane[i, j] = p.transform;
            }
        }

        print(platforms.Count);

        //See if we can hit floor
        foreach (Transform t in platforms.ToList())
        {
            if (Physics.SphereCast(t.position, platformObjWidth / 4, Vector3.down, out RaycastHit hit))
            {
                if (!hit.collider.tag.Equals("Ground"))
                {
                    Destroy(t.gameObject);
                    platforms.Remove(t);
                }
                else
                {
                    t.position = hit.point;
                }
            }
            else
            {
                Destroy(t.gameObject);
                platforms.Remove(t);
            }
        }

        print(platforms.Count);
        Invoke(nameof(Yeet), 0.1f);
    }

    private async void Yeet()
    {
        var cam = Camera.main;
        //Yeet ones we can't see
        foreach (Transform t in platforms.ToList())
        {
            if (Physics.Linecast(cam.transform.position + (Vector3.up * 2), t.transform.position, out RaycastHit hit))
            {
                // Debug.DrawLine(cam.transform.position, hit.point, Color.blue, 999);
                if (hit.transform.gameObject.GetInstanceID() != t.gameObject.GetInstanceID()
                    ||  !t.GetComponent<MeshRenderer>().isVisible)
                {
                    // Debug.DrawLine(cam.transform.position, t.transform.position, Color.blue, 999);
                    // Debug.DrawLine(t.position, t.position + t.up * 1, Color.red, 9999);
                    Destroy(t.gameObject);
                    platforms.Remove(t);
                }
            }
            else
            {
                Debug.Log("linecast fucked");
            }
        }
        
        var models = await APIManager.instance.GetPopular(platforms.Count + Exclude.Length);
        print("Model count " + models.Length);
        
        //exclude models
        models = Array.FindAll(models, (Model m) => !Exclude.Contains(m.Id)).ToArray();
        
        var couch = await APIManager.instance.GetModelByID("75h3mi6uHuC");
        var kata = await APIManager.instance.GetModelByID("zV3WXbyjMf");
        var astra = await APIManager.instance.GetModelByID("dLHpzNdygsg");
        // var models = new Model[] {couch, kata, astra};

        var modelTaskDone = new bool[models.Length];
        var order = spiralOrder(plane);
        order.Reverse(); //start with most popular in center
        var objRef = this;
        for (int i = 0; i < platforms.Count; i++)
        {
            var i1 = i;
            // await UniTask.DelayFrame(1);

            var m = await APIManager.instance.MakeModel(models[i], platformObjWidth * 0.9f, true);
            if (m == null) continue; //Model is fucked
            
            //Find a non null platform to use lol
            var platform = order[objRef._nextPlat];
            objRef._nextPlat++;

            // var piv = platform.transform.GetChild(0);
            while (platform == null || platform.transform.GetChild(0).childCount > 0)
            {
                platform = order[objRef._nextPlat];
                objRef._nextPlat++;
            }

            await UniTask.DelayFrame(1); //wait for box collider to compute
            var box = m.GetComponent<BoxCollider>();

            var newPiv = Instantiate(new GameObject(models[i1].Id), platform.transform.GetChild(0));
            await UniTask.DelayFrame(1);
            
            newPiv.transform.position = box.bounds.center + (Vector3.down * box.bounds.size.y / 2);
            
            // await UniTask.DelayFrame(1);
            m.transform.SetParent(newPiv.transform, true);
            
            await UniTask.DelayFrame(1);
            newPiv.transform.localPosition = Vector3.zero;
            var rot = APIManager.OrbitToRotation(models[i1].Orbit);

            newPiv.transform.rotation = rot;
            
            modelTaskDone[i1] = true;
        }

        //wait for all models to load
        for (int i = 0; i < modelTaskDone.Length; i++)
        {
            if (modelTaskDone[i] != true)
            {
                i = 0;
                await UniTask.DelayFrame(5);
            }
        }

        //await UniTask.Delay(1000 * 5);

        //set animation delay
        var center = new Vector2(width / 2, (height - 1) / 2);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var t = plane[i, j];
                if (t == null) continue;
                var scale = t.GetComponentInChildren<ScaleUp>();
                scale.animationDelay = (new Vector2(i, j) - center).magnitude * spawnerDelaySpacing;
                scale.enabled = true;
            }
        }
    }

    //https://www.geeksforgeeks.org/print-a-given-matrix-in-spiral-form/
    public static List<T> spiralOrder<T>(T[,] matrix)
    {
        List<T> ans = new List<T>();

        if (matrix.Length == 0)
            return ans;

        int R = matrix.GetLength(0), C = matrix.GetLength(1);
        bool[,] seen = new bool[R, C];
        int[] dr = {0, 1, 0, -1};
        int[] dc = {1, 0, -1, 0};
        int r = 0, c = 0, di = 0;

        // Iterate from 0 to R * C - 1
        for (int i = 0; i < R * C; i++)
        {
            ans.Add(matrix[r, c]);
            seen[r, c] = true;
            int cr = r + dr[di];
            int cc = c + dc[di];

            if (0 <= cr && cr < R && 0 <= cc && cc < C
                && !seen[cr, cc])
            {
                r = cr;
                c = cc;
            }
            else
            {
                di = (di + 1) % 4;
                r += dr[di];
                c += dc[di];
            }
        }

        return ans;
    }

    // Update is called once per frame
    void Update()
    {
    }
}