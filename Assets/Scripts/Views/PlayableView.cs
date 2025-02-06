using UnityEngine;

namespace MyTapMatch
{
    public class PlayableView : CellView 
    { 
        public void SetColor(Cell cell)
        {
            Renderer.color = new Color(cell.r, cell.g, cell.b, cell.a);
        }  
    }
}