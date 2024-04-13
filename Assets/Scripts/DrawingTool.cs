using System;
using System.Collections;
using System.Collections.Generic;
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
    [Header("Properties of Interaction")]
    [SerializeField] private float drawingThreshold = 0.1f;
    [SerializeField] private float snappingThreshold = 0.5f;
    [SerializeField] private float maxPluckDistance = 5f; 
    [Header("References")]
    [SerializeField] private Transform polyLinePrefab;
    [SerializeField] private Transform rayIntersectionTransform;
    [SerializeField] private Shapes.Disc rayIntersectionDisc;
    [SerializeField] private Shapes.Disc snappedIntersectionDisc;
    [SerializeField] private Shapes.Polyline pluckLine;
    [SerializeField] private Shapes.Polyline rayLine;
    
    private bool mouseIsDown = false;
    
    public Drawing drawing;

    [SerializeField] private RayCaster rayCaster = new RayCaster();

    private void Awake()
    {
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

    private Vector3 snappedWorldPosition;
    
    
    private int snappedLineIndex;
    private int snappedPointIndex;

    private bool foundSnappingPoint = false;

    private float orig_snappingThreshold;
 
    
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
    
    private void Update()
    {
        mouseIsDown = Input.GetMouseButton(0);
        rightMouseButtonDown = Input.GetMouseButton(1);
        
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

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

            rayCaster.CastRay(snappedWorldPosition, snappedWorldPosition - worldMousePosition(), drawing);

        }
        else if (Input.GetMouseButton(1))
        {
            
            if (!Input.GetKey(KeyCode.LeftShift)) SnappedPoint(true);

            
            List<Vector3> points = new List<Vector3>()
            {
                worldMousePosition(),
                snappedWorldPosition
            };


            
            snappedIntersectionDisc.transform.position = snappedWorldPosition;
            
            
            
            pluckLine.SetPoints(points, new List<Color>()
            {
                Color.Lerp(Color.green, Color.red, Vector3.Distance(points[0], points[1]) / maxPluckDistance),
                Color.Lerp(Color.green, Color.yellow, Vector3.Distance(points[0], points[1]) / maxPluckDistance),

            });

            rayCaster.CastRay(snappedWorldPosition, snappedWorldPosition - worldMousePosition(), drawing);

        }
        else if (Input.GetMouseButtonUp(1))
        {
            snappingThreshold = orig_snappingThreshold;
            playingNote = false;

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
