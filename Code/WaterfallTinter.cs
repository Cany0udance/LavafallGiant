using Godot;

namespace LavafallGiant.Code;

public partial class WaterfallTinter : Node
{
    private Node2D _parent;
    private Color _tint = new Color(1.0f, 0.3f, 0.3f, 1.0f);
    
    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
    }
    
    public override void _Process(double delta)
    {
            
        foreach (var child in _parent.GetChildren())
        {
            var node = child as Node2D;
            if (node == null || !node.Name.ToString().Contains("SpineMesh"))
                continue;

            if (node.Modulate != _tint)
            {
                node.Modulate = _tint;
            }
        }
    }
}