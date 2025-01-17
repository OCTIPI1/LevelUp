﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace LevelUp;

[Serializable]
public class FleckAnimation : IAnimation
{
    private static readonly List<FleckDef> allFlecks;

    private FleckDef fleckDef = null!;
    private Graphic_Single graphic = null!;
    private Texture2D texture = null!;
    private AnimationDefExtension animationExtension = null!;
    public static IEnumerable<IAnimation> Animations => allFlecks.Select(x => new FleckAnimation(x));

    public FleckDef FleckDef
    {
        get => fleckDef;
        set
        {
            fleckDef = value;
            Prepare();
        }
    }

    static FleckAnimation()
    {
        allFlecks = DefDatabase<FleckDef>
           .AllDefs.Where(x => x.HasModExtension<AnimationDefExtension>()).ToList();
    }

    public FleckAnimation()
    {
        fleckDef = allFlecks.RandomElement();
        Prepare();
    }

    private FleckAnimation(FleckDef def)
    {
        fleckDef = def;
        Prepare();
    }

    public string Label => fleckDef.LabelCap;

    public void Prepare()
    {
        animationExtension = fleckDef.GetModExtension<AnimationDefExtension>();

        graphic = fleckDef.graphicData.Graphic is Graphic_Single graphicSingle
            ? graphicSingle
            : throw new InvalidOperationException("Null graphic on FleckDef.");

        texture = ContentFinder<Texture2D>.Get(fleckDef.graphicData.texPath);
    }

    public void Execute(LevelingInfo levelingInfo)
    {
        var pawn = levelingInfo.Pawn;
        var pawnMap = pawn.Map;

        if (pawnMap != Find.CurrentMap)
        {
            return;
        }

        var fleckData = FleckMaker.GetDataStatic(pawn.DrawPos, pawnMap, fleckDef);
        fleckData.exactScale = animationExtension.ExactScale;
        fleckData.scale = animationExtension.Scale;
        fleckData.rotation = animationExtension.Rotation;
        fleckData.rotationRate = animationExtension.RotationRate;
        fleckData.solidTimeOverride = animationExtension.SolidTimeOverride;
        fleckData.airTimeLeft = animationExtension.AirTimeLeft;
        fleckData.targetSize = animationExtension.TargetSize;
        fleckData.velocity = animationExtension.Velocity;
        fleckData.velocityAngle = animationExtension.VelocityAngle;
        fleckData.velocitySpeed = animationExtension.VelocitySpeed;
        fleckData.instanceColor = animationExtension.InstanceColor;
        fleckData.link = new FleckAttachLink(pawn);

        pawnMap.flecks.CreateFleck(fleckData);
    }

    public void DrawGraphic(Rect rect)
    {
        Widgets.DrawTextureFitted(
            outerRect: rect,
            tex: texture,
            scale: 1f,
            texProportions: new Vector2(texture.width, texture.height),
            texCoords: new Rect(0, 0, 1, 1),
            animationExtension.Rotation,
            graphic.MatSingle);
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref fleckDef, "fleckDef");
    }
}
