using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileState
{
    void StruckedByLightning();
    void RainedOn();
    bool IsEnabled { get; set; }
}
