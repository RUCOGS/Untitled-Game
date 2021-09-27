using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Navmap2D;

public interface IPathfinder
{
    Navmap2DPath FindPath(Navmap2D navmap, MapNode from, MapNode to);
}
