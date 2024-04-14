using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;


[Serializable]
public class Line
{
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

    }

}
[Serializable]

public class Drawing
{
    public List<Line> lines = new List<Line>();
}

public class DrawingTool : MonoBehaviour
{

    [Header("Test Material")] 
    public MaterialRegistrySO registry;
    public MaterialRegistrySO.ObjectMaterial testMaterialStart;
    
     
    [Header("Properties of Interaction")]
    [SerializeField] private float drawingThreshold = 0.1f;
    [SerializeField] private float snappingThreshold = 0.5f;
    [SerializeField] private float maxPluckDistance = 5f;

    [Header("Audio Drivers")] public float tremoloFrequencyMax = 2f;
    public float tremoloFrequencyVisualMax;
    

    [Header("Visulisations")] public float tremologAmpMaxSin = 1f;
    public float tremoloAmpVisualMax;
    
    [Header("References")]
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
        testMaterialStart = registry.materials.First((m) => m.type == testMaterialStart.type);
        
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
 
    public static PlayData GetPlayData(Hit lastHit, int tremoloFrequency, float tremoloAmplitude, float vol)
    {
        
        return new PlayData
        {
            cutoff = Mathf.RoundToInt(lastHit.cutOff), // 100 - 8000
            attack =  lastHit.attack, // 0.1 - 1
            decay = lastHit.decay, // 0.1 - 1
            tremoloFrequency = tremoloFrequency, // .5 - 20
            tremoloAmplitude = tremoloAmplitude, // 0.1 - 1
            vol = vol,
        };
    }
    
    private void SnappedPoint(bool withThreshold = true)
    {
        Vector3 closestPoint = Vector3.positiveInfinity;
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
                    closestPoint = _closest;
                    snappedLineIndex = l;
                    snappedPointIndex = i;
                    foundSnappingPoint = true;
                }
            }
        }

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
    
    private void Update()
    {
        mouseIsDown = Input.GetMouseButton(0);
        rightMouseButtonDown = Input.GetMouseButton(1);

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
        
        

        if (Input.GetMouseButtonDown(1) && drawing.lines.Count > 0)
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

            Hit lastHit = rayCaster.CastRay(snappedWorldPosition, snappedWorldPosition - worldMousePosition(), drawing, testMaterialStart);
            
            initialDirection = (worldMousePosition() - snappedWorldPosition).normalized;
            
            synthPlayer.Play(GetPlayData(lastHit, 0, 0f, 1f));
            
        }
        else if (Input.GetMouseButton(1))
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

            Hit hitInfo = rayCaster.CastRay(snappedWorldPosition, snappedWorldPosition - worldMousePosition(), drawing, testMaterialStart);
            
            Debug.Log("Tremolo : " + tremoloFrequency.ToString() + " " + tremoloAmplitude.ToString());
            
            _playData = GetPlayData(hitInfo, Mathf.RoundToInt(tremoloFrequency), tremoloAmplitude, percentage);

            synthPlayer.UpdateSynth(_playData, Time.deltaTime);

            
        }
        else if (Input.GetMouseButtonUp(1))
        {
            snappingThreshold = orig_snappingThreshold;
            playingNote = false;
            synthPlayer.Stop();
        }
        

        if (Input.GetMouseButtonDown(0))
        {
            // First Frame
            snappingThreshold = orig_snappingThreshold;


            Transform polyLine = Transform.Instantiate(polyLinePrefab);

            lineCompleted = false;
            activeLine = new Line(polyLine.GetComponentInChildren<Polyline>());
            activeLine.points = new List<Vector3>();
            
            drawing.lines.Add(activeLine);

            
        }
        else if (Input.GetMouseButtonUp(0))
        {
        }
    }

    // Update is called once per frame
    private Vector2 lastInputPoint = Vector2.negativeInfinity;
    void FixedUpdate()
    {
        if (mouseIsDown && !lineCompleted)
        {
            Vector3 point = snappedWorldPosition;
            
            bool addPoint = (lastInputPoint == Vector2.negativeInfinity) || (Vector2.Distance(lastInputPoint, snappedWorldPosition) > drawingThreshold) ;
            if (addPoint) {
                activeLine.AddPoint(point);
                lastInputPoint = snappedWorldPosition;
            }
        }


    }
}
