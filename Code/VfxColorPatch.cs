using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace LavafallGiant.Code;

[HarmonyPatch(typeof(NWaterfallGiantVfx), "_Ready")]
public static class VfxColorPatch
{
    private static readonly Dictionary<string, Texture2D> TintedTextures = new();

    public static void Postfix(NWaterfallGiantVfx __instance)
    {
        var parent = __instance.GetParent<Node2D>();
        
        var timer = parent.GetTree().CreateTimer(0.1);
        timer.Timeout += () => ApplyLavaSkin(parent);
    }
    
    private static void ApplyLavaSkin(Node2D parent)
    {
        
        if (!parent.HasMethod("get_skeleton"))
            return;
            
        var skeleton = parent.Call("get_skeleton").AsGodotObject();
        if (skeleton == null)
            return;
        
        var waterSlotNames = new HashSet<string>
        {
            "body_water_rect", "mouth_water_rect",
            "waterAnim1", "waterAnim2", "waterAnim3", "waterAnim4",
            "waterAnim5", "waterAnim6", "waterAnim7",
            "mouthAnim1", "mouthAnim2"
        };
        
        var lavaColor = new Color(1.0f, 0.3f, 0.1f, 1.0f);
        
        var slots = skeleton.Call("get_slots").AsGodotArray();
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i].AsGodotObject();
            if (slot == null) continue;
            
            var data = slot.Call("get_data").AsGodotObject();
            if (data == null) continue;
            
            var slotName = data.Call("get_name").AsString();
            
            if (waterSlotNames.Contains(slotName))
            {
                slot.Call("set_color", lavaColor);
            }
        }
        
        ApplyParticleTint(parent);
        TintWaterTextures();
    }
    
    private static void ApplyParticleTint(Node2D parent)
    {
        var particlePaths = new[]
        {
            "MistSlot/MistParticles",
            "MistSlot/Droplets",
            "MouthDropletsSlot/MouthDroplets"
        };
    
        var smokeColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    
        foreach (var path in particlePaths)
        {
            var particles = parent.GetNodeOrNull<GpuParticles2D>(path);
            if (particles != null)
            {
                particles.Modulate = smokeColor;
            }
        }
    }

    
    private static void TintWaterTextures()
    {
        var materialPaths = new[]
        {
            "res://materials/vfx/monsters/waterfall_giant_body_water_mat.tres",
            "res://materials/vfx/monsters/waterfall_giant_mouth_water_mat.tres"
        };
    
        foreach (var matPath in materialPaths)
        {
            var mat = GD.Load<ShaderMaterial>(matPath);
            if (mat == null) continue;
        
            TintAndReplace(mat, "waterTex");
            TintAndReplace(mat, "movingTex");
        
            // Slow down the scroll speed
            var currentSpeed = mat.Get("shader_parameter/scrollSpeed").AsVector2();
            mat.SetShaderParameter("scrollSpeed", currentSpeed * 0.4f);
        }
    }
    
    private static void TintAndReplace(ShaderMaterial mat, string paramName)
    {
        var tex = mat.Get($"shader_parameter/{paramName}").AsGodotObject() as Texture2D;
        if (tex == null) return;
        
        var path = tex.ResourcePath;
        
        if (!TintedTextures.ContainsKey(path))
        {
            var image = tex.GetImage();
            if (image == null)
            {
                return;
            }
            
            for (int x = 0; x < image.GetWidth(); x++)
            {
                for (int y = 0; y < image.GetHeight(); y++)
                {
                    var pixel = image.GetPixel(x, y);
                    var tinted = new Color(
                        Math.Clamp(pixel.B + pixel.R * 0.5f, 0f, 1f),
                        pixel.G * 0.4f,
                        pixel.R * 0.2f,
                        pixel.A
                    );
                    image.SetPixel(x, y, tinted);
                }
            }
            
            TintedTextures[path] = ImageTexture.CreateFromImage(image);
        }
        
        mat.SetShaderParameter(paramName, TintedTextures[path]);
    }
}