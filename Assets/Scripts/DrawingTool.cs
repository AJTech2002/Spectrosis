using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;


[Serializable]
public class Line
{
    public MaterialRegistrySO.ObjectMaterial mat;
    public List<Vector3> points = new List<Vector3>();
    private Polyline polyline;
    
    public Line (Polyline polyline)
    {
        this.polyline = polyline;
        this.polyline.SetPoints(points);
    }


    public void AddPoint(Vector3 point)
    {
        
        points.Add(point);
        this.polyline.SetPoints(points);
        this.polyline.Color = mat.color;

    }

}
[Serializable]

public class Drawing
{
    public List<Line> lines = new List<Line>();
    
    
}

public class DrawingTool : MonoBehaviour
{
    //
    // [Header("Test Material")] 
    // public MaterialRegistrySO registry;
    // public MaterialRegistrySO.ObjectMaterial testMaterialStart;
    //

     
    [Header("Properties of Interaction")]
    [SerializeField] private float drawingThreshold = 0.1f;
    [SerializeField] private float snappingThreshold = 0.5f;
    [SerializeField] private float maxPluckDistance = 5f;
    

    [Header("Audio Drivers")] public float tremoloFrequencyMax = 2f;
    public float tremoloFrequencyVisualMax;
    public float maxVol = 2f;
    

    [Header("Visulisations")] public float tremologAmpMaxSin = 1f;
    public float tremoloAmpVisualMax;
    
    [Header("References")]
    [SerializeField] private MaterialManager _materialManager;

    [SerializeField] private Transform polyLinePrefab;
    [SerializeField] private Transform rayIntersectionTransform;
    [SerializeField] private Shapes.Disc rayIntersectionDisc;
    [SerializeField] private Transform snappedIntersectionDisc;
    [SerializeField] private DashedLineDrawer pluckLine;
    [SerializeField] private Shapes.Polyline rayLine;
    [SerializeField] private SynthPlayer synthPlayer;
    [SerializeField] private TremoloCalculator tremoloCalculator;
    [SerializeField] private Disc tremoloDisc;
    
    private bool mouseIsDown = false;
    
    public Drawing drawing;

    [SerializeField] private RayCaster rayCaster = new RayCaster();

    private void Awake()
    {
        // testMaterialStart = registry.materials.First((m) => m.type == testMaterialStart.type);
        
        drawing.lines = new List<Line>();
        orig_snappingThreshold = snappingThreshold;
    }

    private Line activeLine;
    private bool lineCompleted = false;

    private Vector3 mousePosition;

    private Vector3 worldMousePosition()
    {
        return new Vector3(mousePosition.x, mousePosition.y, 0);
    }

    private Vector3 realWorldMousePosition()
    {
        Vector3 mP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mP.z = 0f;
        return mP;
    }

    private Vector3 snappedWorldPosition;
    
    
    private int snappedLineIndex;
    private int snappedPointIndex;

    private bool foundSnappingPoint = false;

    private float orig_snappingThreshold;

    [Header("Debugging")]
    [SerializeField] private PlayData lastPlayData;
    [SerializeField] private Hit lastHitData;
    [SerializeField] private float outputVol;
    [SerializeField] private float shapeVolume;
    [SerializeField] private int tremoloFreq;

    public PlayData GetPlayData(Hit lastHit, int _tremoloFrequency, float vol, float volOfShape)
    {
        
        PlayData newData = new PlayData
        {
            cutoff = Mathf.RoundToInt(lastHit.cutOff), // 100 - 8000
            attack =  lastHit.attack, // 0.1 - 1
            decay = lastHit.decay, // 0.1 - 1
            release = lastHit.release,
            sustain = lastHit.sustain,
            tremoloFrequency = tremoloFrequency, // .5 - 20
            tremoloAmplitude = 5f, // 0.1 - 1
            vol = vol,
        };

        
        // Just for debugging so you can see the values in inspector realtime to debug 
        lastPlayData = newData;
        lastHitData = lastHit;
        outputVol = vol;
        shapeVolume = volOfShape;
        tremoloFreq = _tremoloFrequency;
        
        return newData;
    }

    private Line snappingLine = null;
    private void SnappedPoint(bool withThreshold = true)
    {
        Vector3 closestPoint = Vector3.positiveInfinity;
        Line closestLine = null;
        foundSnappingPoint = false;

        for (int l = 0; l < drawing.lines.Count; l++)
        {
            Line line = drawing.lines[l];
            for (int i = 0; i < line.points.Count; i++)
            {
                if (i == 0) continue;;
                
                Vector3 pointA = line.points[i];
                Vector3 pointB = line.points[i - 1];
                
                Vector3 _closest = RayMath.ClosestPointOnLineSegment(pointA, pointB, worldMousePosition());
                
                if (Vector3.Distance(_closest, worldMousePosition()) < Vector3.Distance(closestPoint, worldMousePosition()))
                {
                    closestLine = line;
                    closestPoint = _closest;
                    snappedLineIndex = l;
                    snappedPointIndex = i;
                    foundSnappingPoint = true;
                }
            }
        }

        snappingLine = closestLine;
        
        if (withThreshold)
        {
            if (Vector3.Distance(closestPoint, worldMousePosition()) > snappingThreshold)
            {
                foundSnappingPoint = false;
            }
        }

        if (!foundSnappingPoint) snappedWorldPosition = worldMousePosition();
        else snappedWorldPosition = closestPoint;

    }

    private bool rightMouseButtonDown = false;

    private bool playingNote = false;

    private Vector3 initialDirection;

    private float lastAngle;
    private float lastOscillationTime;

    private float tremoloAmplitude;
    private float tremoloFrequency;

    [Header("Debug")] [SerializeField] private PlayData _playData;

    private List<Vector3> mousePositions = new List<Vector3>();

    private Vector3 startPlayingWorldPositionSnapped;
    private Vector3 startPlayingWorldMousePosition;
    
    bool IsMouseOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
    
    private void Update()
    {
        mouseIsDown = !IsMouseOverUI() &&  Input.GetMouseButton(0);
        rightMouseButtonDown = !IsMouseOverUI() && Input.GetMouseButton(1);

        if (!Input.GetKey(KeyCode.LeftControl)) mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        tremoloDisc.gameObject.SetActive(Input.GetKey(KeyCode.LeftControl));
        tremoloDisc.transform.position = snappedWorldPosition;
        // tremoloDisc.transform.right = (mousePosition - snappedWorldPosition).normalized;
        
        if (!playingNote) SnappedPoint(true);
        
        // Mouse Cursor
        
        if (mouseIsDown || rightMouseButtonDown) rayIntersectionDisc.Color = Color.green;
        else rayIntersectionDisc.Color = Color.red;
        
        if (playingNote) rayIntersectionDisc.Color = Color.cyan;

        if (!playingNote)
            rayIntersectionTransform.transform.position = Vector3.Lerp(rayIntersectionTransform.transform.position,
                snappedWorldPosition, 20 * Time.deltaTime);
        else rayIntersectionTransform.transform.position = worldMousePosition();

        snappedIntersectionDisc.gameObject.SetActive(foundSnappingPoint && rightMouseButtonDown);
        pluckLine.gameObject.SetActive(playingNote);
        rayLine.gameObject.SetActive(rightMouseButtonDown);

        if (!IsMouseOverUI() && Input.GetMouseButtonDown(1) && drawing.lines.Count > 0)
        {
            mousePositions.Clear();
            int totalCount = 0;
            for (int i = 0; i < drawing.lines.Count; i++)
            {
                totalCount += drawing.lines[i].points.Count;
            }

            if (totalCount > 0)
            {
                snappingThreshold = Mathf.Infinity;
                playingNote = true;
                SnappedPoint(true);
            }
            
            List<Hit> hits = new List<Hit>();
            // Play first note
            
            float percentage = Vector3.Distance(realWorldMousePosition(), snappedWorldPosition) / maxPluckDistance;


            Hit lastHit = rayCaster.CastRay(snappedWorldPosition, snappedWorldPosition - worldMousePosition(), drawing, snappingLine.mat, percentage * maxVol, rayLine);
            
            initialDirection = (worldMousePosition() - snappedWorldPosition).normalized;
            
            synthPlayer.Play(GetPlayData(lastHit, 0,  1f, 0f));
            
        }
        else if (!IsMouseOverUI() &&Input.GetMouseButton(1))
        {
            
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl)) SnappedPoint(true);

            
            List<Vector3> points = new List<Vector3>()
            {
                worldMousePosition(),
                snappedWorldPosition
            };

            if (Input.GetKey(KeyCode.LeftControl))
            {
                tremoloDisc.transform.right = (worldMousePosition() - snappedWorldPosition).normalized;
                tremoloDisc.AngRadiansStart = 0;
                tremoloDisc.AngRadiansEnd = Vector3.SignedAngle(
                    worldMousePosition() - snappedWorldPosition,
                    realWorldMousePosition() - snappedWorldPosition,
                    Vector3.forward
                ) * Mathf.Deg2Rad;

                tremoloDisc.Radius = (realWorldMousePosition() - snappedWorldPosition).magnitude;

                tremoloFrequency = (Mathf.Abs(Vector3.SignedAngle(
                    worldMousePosition() - snappedWorldPosition,
                    realWorldMousePosition() - snappedWorldPosition,
                    Vector3.forward
                )) / 180f) * 10f;

                tremoloAmplitude = 10f;
            }
            else
            {
                tremoloFrequency = 0f;
                tremoloAmplitude = 0f;
            }
            
            snappedIntersectionDisc.transform.position = snappedWorldPosition;
            
            
            // snappedIntersectionDisc.transform.rotation = Quaternion.LookRotation(worldMousePosition() - snappedIntersectionDisc.transform.position, snappedIntersectionDisc.transform.forward);
            // snappedIntersectionDisc.transform.(worldMousePosition());
            
            snappedIntersectionDisc.right = worldMousePosition() - snappedWorldPosition;
            
            Vector3 direction = (worldMousePosition() - snappedWorldPosition).normalized;

            List<Color> colors = new List<Color>();

            
            
            float percentage = Vector3.Distance(realWorldMousePosition(), points[1]) / maxPluckDistance;
            
            
            Color aColor = Color.Lerp(Color.green, Color.red,
                percentage);

            Color bColor = Color.Lerp(Color.green, Color.yellow,
                percentage);

        
            colors.Add(aColor);
            colors.Add(bColor);



            pluckLine.SetPoints(points,colors);

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                startPlayingWorldMousePosition = worldMousePosition();
                startPlayingWorldPositionSnapped = snappedWorldPosition;
            }

            Hit hitInfo = rayCaster.CastRay(snappedWorldPosition, snappedWorldPosition - worldMousePosition(), drawing, snappingLine.mat,percentage * maxVol, rayLine);
            
            
            _playData = GetPlayData(hitInfo, Mathf.RoundToInt(tremoloFrequency), percentage, (max-min).magnitude);

            synthPlayer.UpdateSynth(_playData, Time.deltaTime);

            
        }
        else if (!IsMouseOverUI() && Input.GetMouseButtonUp(1))
        {
            snappingThreshold = orig_snappingThreshold;
            playingNote = false;
            synthPlayer.Stop();
        }
        

        if (!IsMouseOverUI() && Input.GetMouseButtonDown(0))
        {
            // First Frame
            snappingThreshold = orig_snappingThreshold;


            Transform polyLine = Transform.Instantiate(polyLinePrefab);

            lineCompleted = false;
            activeLine = new Line(polyLine.GetComponentInChildren<Polyline>());
            activeLine.points = new List<Vector3>();
            activeLine.mat = _materialManager.selectedMaterialItem._objectMaterial;
            drawing.lines.Add(activeLine);

            
        }
        else if (!IsMouseOverUI() && Input.GetMouseButtonUp(0))
        {
        }
    }

    // Update is called once per frame
    private Vector2 lastInputPoint = Vector2.negativeInfinity;
    private Vector2 min = Vector3.positiveInfinity;
    private Vector2 max = Vector3.negativeInfinity;
    void FixedUpdate()
    {
        if (mouseIsDown && !lineCompleted)
        {
            Vector3 point = snappedWorldPosition;
            
            bool addPoint = (lastInputPoint == Vector2.negativeInfinity) || (Vector2.Distance(lastInputPoint, snappedWorldPosition) > drawingThreshold) ;
            if (addPoint) {
                activeLine.AddPoint(point);
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
                lastInputPoint = snappedWorldPosition;
            }
        }


    }
}
