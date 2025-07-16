using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class RhythmLevelModel {
    public float[] NoteTimings;
    public int RequiredScore;

    public void Validate() {
        if (NoteTimings == null || NoteTimings.Length == 0)
            throw new ArgumentException("NoteTimings cannot be empty");
    }
}
