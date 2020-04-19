using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileState
{
    void StruckedByLightning();
    void RainedOn();
    void OnMouseOverEvent();
    void OnMouseExitEvent();
    bool IsEnabled { get; set; }
}
