// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace Fabulous.XamarinForms 

[<AutoOpen>]
module SkiaSharpGLExtension =

    open Fabulous
    open Xamarin.Forms
    open SkiaSharp
    open SkiaSharp.Views.Forms

    let PaintGLSurfaceAttribKey = AttributeKey<_> "SKCanvas_PaintSurface"
    let HasRenderLoopAttribKey = AttributeKey<_> "SKCanvas_HasRenderLoop"

    type Fabulous.XamarinForms.View with
        /// Describes a Map in the view
        static member SKGLView(?paintSurface: (SKPaintGLSurfaceEventArgs -> unit), ?touch: (SKTouchEventArgs -> unit), ?hasRenderLoop: bool, ?enableTouchEvents: bool,
                                   ?invalidate: bool,
                                   // inherited attributes common to all views
                                   ?horizontalOptions, ?verticalOptions, ?margin, ?gestureRecognizers, ?anchorX, ?anchorY, ?backgroundColor,
                                   ?height, ?inputTransparent, ?isEnabled, ?isVisible, ?minimumHeight, ?minimumWidth,
                                   ?opacity, ?rotation, ?rotationX, ?rotationY, ?scale, ?style, ?translationX, ?translationY, ?width,
                                   ?resources, ?styles, ?styleSheets, ?classId, ?styleId, ?automationId, ?created, ?styleClasses) =

            // Count the number of additional attributes
            let attribCount = 0
            let attribCount = match hasRenderLoop with Some _ -> attribCount + 1 | None -> attribCount
            let attribCount = match enableTouchEvents with Some _ -> attribCount + 1 | None -> attribCount
            //let attribCount = match ignorePixelScaling with Some _ -> attribCount + 1 | None -> attribCount
            let attribCount = match paintSurface with Some _ -> attribCount + 1 | None -> attribCount
            let attribCount = match touch with Some _ -> attribCount + 1 | None -> attribCount

            // Populate the attributes of the base element
            let attribs = 
                ViewBuilders.BuildView(attribCount, ?horizontalOptions=horizontalOptions, ?verticalOptions=verticalOptions, 
                                       ?margin=margin, ?gestureRecognizers=gestureRecognizers, ?anchorX=anchorX, ?anchorY=anchorY, 
                                       ?backgroundColor=backgroundColor, ?height=height, ?inputTransparent=inputTransparent, 
                                       ?isEnabled=isEnabled, ?isVisible=isVisible, ?minimumHeight=minimumHeight,
                                       ?minimumWidth=minimumWidth, ?opacity=opacity, ?rotation=rotation, 
                                       ?rotationX=rotationX, ?rotationY=rotationY, ?scale=scale, ?style=style, 
                                       ?translationX=translationX, ?translationY=translationY, ?width=width, 
                                       ?resources=resources, ?styles=styles, ?styleSheets=styleSheets, ?classId=classId, ?styleId=styleId,
                                       ?automationId=automationId, ?created=created, ?styleClasses=styleClasses)

            // Add our own attributes. They must have unique names which must match the names below.
            match hasRenderLoop with None -> () | Some v -> attribs.Add(HasRenderLoopAttribKey, v)
            match enableTouchEvents with None -> () | Some v -> attribs.Add(CanvasEnableTouchEventsAttribKey, v) 
            //match ignorePixelScaling with None -> () | Some v -> attribs.Add(IgnorePixelScalingAttribKey, v) 
            match paintSurface with None -> () | Some v -> attribs.Add(PaintGLSurfaceAttribKey, System.EventHandler<_>(fun _sender args -> v args))
            match touch with None -> () | Some v -> attribs.Add(TouchAttribKey, System.EventHandler<_>(fun _sender args -> v args))

            // The create method
            let create () = new SkiaSharp.Views.Forms.SKGLView()

            // The update method
            let update (prevOpt: ViewElement voption) (source: ViewElement) (target: SKGLView) = 
                ViewBuilders.UpdateView (prevOpt, source, target)
                source.UpdatePrimitive(prevOpt, target, HasRenderLoopAttribKey, (fun target v -> target.HasRenderLoop <- v))
                source.UpdatePrimitive(prevOpt, target, CanvasEnableTouchEventsAttribKey, (fun target v -> target.EnableTouchEvents <- v))
                //source.UpdatePrimitive(prevOpt, target, IgnorePixelScalingAttribKey, (fun target v -> target.IgnorePixelScaling <- v))
                source.UpdateEvent(prevOpt, PaintGLSurfaceAttribKey, target.PaintSurface)
                source.UpdateEvent(prevOpt, TouchAttribKey, target.Touch)
                if invalidate = Some true then target.InvalidateSurface()

            // The element
            ViewElement.Create(create, update, attribs)

#if DEBUG 
    type State = 
        { mutable touches: int
          mutable paints: int }

    let sample1 = 
        View.Stateful(
            (fun () -> { touches = 0; paints = 0 }), 
            (fun state -> 
                View.SKGLView(enableTouchEvents = true, 
                    paintSurface = (fun args -> 
                        let width = (float32)args.BackendRenderTarget.Width
                        let height = (float32)args.BackendRenderTarget.Height
                        let surface = args.Surface
                        let canvas = surface.Canvas
                        state.paints <- state.paints + 1
                        printfn "paint event, total paints on this control = %d" state.paints

                        canvas.Clear() 
                        use paint = new SKPaint(Style = SKPaintStyle.Stroke, Color = Color.Red.ToSKColor(), StrokeWidth = 25.0f)
                        canvas.DrawCircle(width / 2.0f, height / 2.0f, 100.0f, paint)
                    ),
                    touch = (fun args -> 
                        state.touches <- state.touches + 1
                        printfn "touch event at (%f, %f), total touches on this control = %d" args.Location.X args.Location.Y state.touches
                    )
            )))
#endif
