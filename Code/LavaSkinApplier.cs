using Godot;

namespace LavafallGiant.Code;

public partial class LavaSkinApplier : Node
{
    private Node2D _parent;
    private bool _applied = false;
    
    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
    }
    
    public override void _Process(double delta)
    {
        ApplyWaterShader(_parent);
        
        if (!_applied)
        {
            ApplyParticleTint(_parent);
            _applied = true;
        }
    }
    
    private void ApplyWaterShader(Node node)
    {
        if (node is CanvasItem ci && ci.Material is ShaderMaterial sm)
        {
            if (sm.Shader?.ResourcePath?.Contains("waterfall") == true)
            {
                var currentColor = sm.Get("shader_parameter/ColorParameter").AsColor();
                
                // Check if already lava-colored (red > green)
                if (currentColor.R <= currentColor.G)
                {
                    var brightness = currentColor.G;
                    var lavaColor = new Color(brightness * 2f, brightness * 0.3f, brightness * 0.1f, 1.0f);
                    sm.Set("shader_parameter/ColorParameter", lavaColor);
                    
                    var scrollSpeed = sm.Get("shader_parameter/scrollSpeed").AsVector2();
                    if (scrollSpeed.Y < -1.5f) // Only slow if not already slowed
                    {
                        sm.Set("shader_parameter/scrollSpeed", scrollSpeed * 0.5f);
                    }
                }
            }
        }
        
        foreach (var child in node.GetChildren())
        {
            ApplyWaterShader((Node)child);
        }
    }
    
    private void ApplyParticleTint(Node2D parent)
    {
        var particlePaths = new[]
        {
            "MistSlot/MistParticles",
            "MistSlot/Droplets",
            "MouthDropletsSlot/MouthDroplets"
        };
        
        var redTint = new Color(1.0f, 0.4f, 0.4f, 1.0f);
        
        foreach (var path in particlePaths)
        {
            var particles = parent.GetNodeOrNull<GpuParticles2D>(path);
            if (particles != null)
            {
                particles.Modulate = redTint;
            }
        }
    }
}