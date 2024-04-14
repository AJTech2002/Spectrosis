using System.Collections.Generic;
using UnityEngine;

public class TremoloCalculator : MonoBehaviour
{
   private List<float> timeStamps = new List<float>();
    private List<float> positions = new List<float>();
    private float lastTimeChecked;
    public float frequency;
    public float amplitude;
    private float targetFrequency = 0;
    private float targetAmplitude = 0;
    
    public float amplitudeMultiplier = 0.1f;
    public float frequencyMultiplier = 0.1f;
    
    private bool isDragging = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetMouseButtonDown(1))
            {
                // Start of new drag
                isDragging = true;
                timeStamps.Clear();
                positions.Clear();
            }

            if (Input.GetMouseButton(1))
            {
                // During dragging
                timeStamps.Add(Time.time);
                positions.Add(Input.mousePosition.x);
                PruneOldData();

                if (positions.Count > 1)
                {
                    CalculateFrequencyAndAmplitude(); // Continuously calculate during dragging
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                // End of drag
                isDragging = false;
            }
        }
        else
        {
            isDragging = false;
        }

        // Lerp to zero when not dragging
        if (!isDragging)
        {
            targetFrequency = 0;
            targetAmplitude = 0;
        }

        // Smoothly update frequency and amplitude
        frequency = Mathf.Lerp(frequency, targetFrequency, Time.deltaTime * 10f);
        amplitude = Mathf.Lerp(amplitude, targetAmplitude, Time.deltaTime * 10f);
        
        
    }
    
    private void PruneOldData()
    {
        float currentTime = Time.time;
        while (timeStamps.Count > 0 && currentTime - timeStamps[0] > 2.0f)
        {
            // Remove data older than 2 seconds
            timeStamps.RemoveAt(0);
            positions.RemoveAt(0);
        }
    }

    private void CalculateFrequencyAndAmplitude()
    {
        float totalDistance = 0;
        int peakCount = 0;
        float lastPosition = positions[0];

        for (int i = 1; i < positions.Count; i++)
        {
            totalDistance += Mathf.Abs(positions[i] - positions[i - 1]);
            if ((positions[i] > lastPosition && positions[i - 1] < positions[i]) || 
                (positions[i] < lastPosition && positions[i - 1] > positions[i]))
            {
                peakCount++;
            }
            lastPosition = positions[i];
        }

        // Calculate amplitude and frequency based on total distance and peak count
        targetAmplitude = (totalDistance * amplitudeMultiplier) / positions.Count;
        if (timeStamps.Count > 1)
        {
            float totalTime = timeStamps[timeStamps.Count - 1] - timeStamps[0];
            targetFrequency = (peakCount / totalTime) * frequencyMultiplier;
        }
    }
}