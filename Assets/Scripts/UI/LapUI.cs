using System;
using TMPro;
using UnityEngine;

namespace UI
{
public class LapUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerTotal;
    [SerializeField] private Transform[] _lapRows;
    [SerializeField] private TextMeshProUGUI[] _lapTime;
    [SerializeField] private TextMeshProUGUI _lapCount;
    private TimeSpan[] _lapTotals;
    private DateTime _startTime;
    private short _lapsCounted;
    private short _maxLaps;

    private void Start()
    {
        //Setup(7);
    }
    public void Setup(short laps)
    {
        _maxLaps = laps;
        _timerTotal.text = "0:00:00";
        _lapsCounted = 0;
        _lapCount.text = "1/" + _maxLaps;
        for (int i = 0; i < _maxLaps; i++)
        {
            _lapTime[i].text = "0:00:00";
            _lapRows[i].gameObject.SetActive(false);
        }
        _lapTotals = new TimeSpan[_maxLaps];
        var now = DateTime.Now;
        SetStartTime(now);
    }
    private void Update()
    {
        if (_lapsCounted >= _maxLaps) return;
        var now = DateTime.Now;
        SetRaceTime(now);
    }
    public void SetStartTime(DateTime time)
    {
        _startTime = time;
    }
    public void SetRaceTime(DateTime time)
    {
        DateTime currentTime = new DateTime(time.Ticks - _startTime.Ticks);
        TimeSpan timeSpan = currentTime.TimeOfDay;
        _timerTotal.text = String.Format(@"{0:mm\:ss\:ff}", timeSpan);
    }
    public void FinishLap(DateTime time)
    {
        if (_lapsCounted >= _maxLaps) return;
        TimeSpan currentTime = new TimeSpan(time.Ticks - _startTime.Ticks);
        _lapRows[_lapsCounted].gameObject.SetActive(true);
        if (_lapsCounted == 0) {
            _lapTime[_lapsCounted].text = String.Format(@"{0:mm\:ss\:ff}", currentTime);
        } else {
            _lapTime[_lapsCounted].text = String.Format(@"{0:mm\:ss\:ff}", currentTime - _lapTotals[_lapsCounted - 1]);
        }
        _lapTotals[_lapsCounted] = currentTime;
        _lapsCounted++;
        _lapCount.text = (_lapsCounted+1) + "/" + _maxLaps;
    }
}
}