using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDescription
{
    double PowerLevel();
    Alignment GetAlignment();
    string CardText();
}
