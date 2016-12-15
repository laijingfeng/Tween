// LeanTween version 2.42 - http://dentedpixel.com/developer-diary/
//
// The MIT License (MIT)
//
// Copyright (c) 2016 Russell Savage - Dented Pixel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.




using UnityEngine;
using System;
using System.Collections.Generic;

public enum TweenAction
{
    MOVE_X,
    MOVE_Y,
    MOVE_Z,
    MOVE_LOCAL_X,
    MOVE_LOCAL_Y,
    MOVE_LOCAL_Z,
    MOVE_CURVED,
    MOVE_CURVED_LOCAL,
    MOVE_SPLINE,
    MOVE_SPLINE_LOCAL,
    SCALE_X,
    SCALE_Y,
    SCALE_Z,
    ROTATE_X,
    ROTATE_Y,
    ROTATE_Z,
    ROTATE_AROUND,
    ROTATE_AROUND_LOCAL,
    CANVAS_ROTATEAROUND,
    CANVAS_ROTATEAROUND_LOCAL,
    CANVAS_PLAYSPRITE,
    ALPHA,
    TEXT_ALPHA,
    CANVAS_ALPHA,
    CANVASGROUP_ALPHA,
    ALPHA_VERTEX,
    COLOR,
    CALLBACK_COLOR,
    TEXT_COLOR,
    CANVAS_COLOR,
    CANVAS_MOVE_X,
    CANVAS_MOVE_Y,
    CANVAS_MOVE_Z,
    CALLBACK,
    MOVE,
    MOVE_LOCAL,
    MOVE_TO_TRANSFORM,
    ROTATE,
    ROTATE_LOCAL,
    SCALE,
    VALUE3,
    GUI_MOVE,
    GUI_MOVE_MARGIN,
    GUI_SCALE,
    GUI_ALPHA,
    GUI_ROTATE,
    DELAYED_SOUND,
    CANVAS_MOVE,
    CANVAS_SCALE,
    CANVAS_SIZEDELTA,
}

public enum LeanTweenType
{
    notUsed, linear, easeOutQuad, easeInQuad, easeInOutQuad, easeInCubic, easeOutCubic, easeInOutCubic, easeInQuart, easeOutQuart, easeInOutQuart,
    easeInQuint, easeOutQuint, easeInOutQuint, easeInSine, easeOutSine, easeInOutSine, easeInExpo, easeOutExpo, easeInOutExpo, easeInCirc, easeOutCirc, easeInOutCirc,
    easeInBounce, easeOutBounce, easeInOutBounce, easeInBack, easeOutBack, easeInOutBack, easeInElastic, easeOutElastic, easeInOutElastic, easeSpring, easeShake, punch, once, clamp, pingPong, animationCurve
}

/// <summary>
/// LeanTween is an efficient tweening engine for Unity3d<br><br>
/// <summary>

public class LeanTween : MonoBehaviour
{

    public static bool throwErrors = true;
    public static float tau = Mathf.PI * 2.0f;
    public static float PI_DIV2 = Mathf.PI / 2.0f;

    private static LTDescr[] tweens;
    private static int[] tweensFinished;
    private static LTDescr tween;
    private static int tweenMaxSearch = -1;
    private static int maxTweens = 400;
    private static int frameRendered = -1;
    private static GameObject _tweenEmpty;
    public static float dtEstimated = -1f;
    public static float dtManual;
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5
    private static float previousRealTime;
#endif
    public static float dtActual;
    private static int i;
    private static int j;
    private static int finishedCnt;
    public static AnimationCurve punch = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.112586f, 0.9976035f), new Keyframe(0.3120486f, -0.1720615f), new Keyframe(0.4316337f, 0.07030682f), new Keyframe(0.5524869f, -0.03141804f), new Keyframe(0.6549395f, 0.003909959f), new Keyframe(0.770987f, -0.009817753f), new Keyframe(0.8838775f, 0.001939224f), new Keyframe(1.0f, 0.0f));
    public static AnimationCurve shake = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f), new Keyframe(0.75f, -1f), new Keyframe(1f, 0f));

    public static void init()
    {
        init(maxTweens);
    }

    public static int maxSearch
    {
        get
        {
            return tweenMaxSearch;
        }
    }

    public static int maxSimulataneousTweens
    {
        get
        {
            return maxTweens;
        }
    }

    /// <summary>
    /// Find out how many tweens you have animating at a given time
    /// <summary>
    public static int tweensRunning
    {
        get
        {
            int count = 0;
            for (int i = 0; i <= tweenMaxSearch; i++)
            {
                if (tweens[i].toggle)
                {
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// This line is optional. Here you can specify the maximum number of tweens you will use (the default is 400).  This must be called before any use of LeanTween is made for it to be effective.
    /// <summary>
    /// <param name="maxSimultaneousTweens">The maximum number of tweens you will use, make sure you don't go over this limit, otherwise the code will throw an error</param>
    public static void init(int maxSimultaneousTweens)
    {
        if (tweens == null)
        {
            maxTweens = maxSimultaneousTweens;
            tweens = new LTDescr[maxTweens];
            tweensFinished = new int[maxTweens];
            _tweenEmpty = new GameObject();
            _tweenEmpty.name = "~LeanTween";
            _tweenEmpty.AddComponent(typeof(LeanTween));
            _tweenEmpty.isStatic = true;
#if !UNITY_EDITOR
            _tweenEmpty.hideFlags = HideFlags.HideAndDontSave;
#endif
            DontDestroyOnLoad(_tweenEmpty);
            for (int i = 0; i < maxTweens; i++)
            {
                tweens[i] = new LTDescr();
            }

#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += onLevelWasLoaded54;
#endif
        }
    }

    public static void reset()
    {
        if (tweens != null)
        {
            for (int i = 0; i <= tweenMaxSearch; i++)
            {
                if (tweens[i] != null)
                    tweens[i].toggle = false;
            }
        }
        tweens = null;
        Destroy(_tweenEmpty);
    }

    public void Update()
    {
        LeanTween.update();
    }

#if UNITY_5_4_OR_NEWER
    private static void onLevelWasLoaded54( UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode ){ internalOnLevelWasLoaded( scene.buildIndex ); }
#else
    public void OnLevelWasLoaded(int lvl) { internalOnLevelWasLoaded(lvl); }
#endif

    private static void internalOnLevelWasLoaded(int lvl)
    {
        // Debug.Log("reseting gui");
        LTGUI.reset();
    }

    private static int maxTweenReached;

    public static void update()
    {
        if (frameRendered != Time.frameCount)
        { // make sure update is only called once per frame
            init();

#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5
            dtEstimated = Time.realtimeSinceStartup - previousRealTime;
            if(dtEstimated>0.2f) // a catch put in, when at the start sometimes this number can grow unrealistically large
            dtEstimated = 0.2f;
            previousRealTime = Time.realtimeSinceStartup;
#else

            dtEstimated = dtEstimated < 0f ? 0f : dtEstimated = Time.unscaledDeltaTime;

            //		Debug.Log("Time.unscaledDeltaTime:"+Time.unscaledDeltaTime);
#endif

            dtActual = Time.deltaTime;
            maxTweenReached = 0;
            finishedCnt = 0;
            // if(tweenMaxSearch>1500)
            //			 Debug.Log("tweenMaxSearch:"+tweenMaxSearch +" maxTweens:"+maxTweens);
            for (int i = 0; i <= tweenMaxSearch && i < maxTweens; i++)
            {
                tween = tweens[i];
                //				if(i==0 && tweens[i].toggle)
                //					Debug.Log("tweens["+i+"]"+tweens[i]);
                if (tween.toggle)
                {
                    maxTweenReached = i;

                    if (tween.updateInternal())
                    { // returns true if the tween is finished with it's loop
                        tweensFinished[finishedCnt] = i;
                        finishedCnt++;
                    }
                }
            }

            // Debug.Log("maxTweenReached:"+maxTweenReached);
            tweenMaxSearch = maxTweenReached;
            frameRendered = Time.frameCount;

            for (int i = 0; i < finishedCnt; i++)
            {
                j = tweensFinished[i];
                tween = tweens[j];
                //				Debug.Log("removing tween:"+tween);
                removeTween(j);
                if (tween.hasExtraOnCompletes && tween.trans != null)
                    tween.callOnCompletes();
            }

        }
    }



    public static void removeTween(int i, int uniqueId)
    { // Only removes the tween if the unique id matches
        if (tweens[i].uniqueId == uniqueId)
        {
            removeTween(i);
        }
    }

    // This method is only used internally! Do not call this from your scripts. To cancel a tween use LeanTween.cancel
    public static void removeTween(int i)
    {
        if (tweens[i].toggle)
        {
            tweens[i].toggle = false;
            //logError("Removing tween["+i+"]:"+tweens[i]);
            if (tweens[i].destroyOnComplete)
            {
                //Debug.Log("destroying tween.type:"+tween.type);
                if (tweens[i].ltRect != null)
                {
                    //	Debug.Log("destroy i:"+i+" id:"+tweens[i].ltRect.id);
                    LTGUI.destroy(tweens[i].ltRect.id);
                }
                else
                { // check if equal to tweenEmpty
                    if (tweens[i].trans != null && tweens[i].trans.gameObject != _tweenEmpty)
                    {
                        Destroy(tweens[i].trans.gameObject);
                    }
                }
            }
            //tweens[i].optional = null;
            startSearch = i;
            //Debug.Log("start search reset:"+startSearch + " i:"+i+" tweenMaxSearch:"+tweenMaxSearch);
            if (i + 1 >= tweenMaxSearch)
            {
                //Debug.Log("reset to zero");
                startSearch = 0;
                //tweenMaxSearch--;
            }
        }
    }

    public static Vector3[] add(Vector3[] a, Vector3 b)
    {
        Vector3[] c = new Vector3[a.Length];
        for (i = 0; i < a.Length; i++)
        {
            c[i] = a[i] + b;
        }

        return c;
    }

    public static float closestRot(float from, float to)
    {
        float minusWhole = 0 - (360 - to);
        float plusWhole = 360 + to;
        float toDiffAbs = Mathf.Abs(to - from);
        float minusDiff = Mathf.Abs(minusWhole - from);
        float plusDiff = Mathf.Abs(plusWhole - from);
        if (toDiffAbs < minusDiff && toDiffAbs < plusDiff)
        {
            return to;
        }
        else
        {
            if (minusDiff < plusDiff)
            {
                return minusWhole;
            }
            else
            {
                return plusWhole;
            }
        }
    }

    /// <summary>
    /// Cancels all tweens
    /// <summary>
    /// <param name="callComplete">(optional) if true, then the all onCompletes will run before canceling</param>
    public static void cancelAll()
    {
        cancelAll(false);
    }
    public static void cancelAll(bool callComplete)
    {
        init();
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].trans != null)
            {
                if (callComplete && tweens[i].optional.onComplete != null)
                    tweens[i].optional.onComplete();
                removeTween(i);
            }
        }
    }

    /// <summary>
    /// Cancel all tweens that are currently targeting the gameObject
    /// <summary>
    /// <param name="gameObject">gameObject whose tweens you wish to cancel</param>
    /// <param name="callOnComplete">(optional) whether to call the onComplete method before canceling</param>
    public static void cancel(GameObject gameObject)
    {
        cancel(gameObject, false);
    }
    public static void cancel(GameObject gameObject, bool callOnComplete)
    {
        init();
        Transform trans = gameObject.transform;
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].toggle && tweens[i].trans == trans)
            {
                if (callOnComplete && tweens[i].optional.onComplete != null)
                    tweens[i].optional.onComplete();
                removeTween(i);
            }
        }
    }

    public static void cancel(GameObject gameObject, int uniqueId)
    {
        if (uniqueId >= 0)
        {
            init();
            int backId = uniqueId & 0xFFFF;
            int backCounter = uniqueId >> 16;
            // Debug.Log("uniqueId:"+uniqueId+ " id:"+backId +" counter:"+backCounter + " setCounter:"+ tweens[backId].counter + " tweens[id].type:"+tweens[backId].type);
            if (tweens[backId].trans == null || (tweens[backId].trans.gameObject == gameObject && tweens[backId].counter == backCounter))
                removeTween((int)backId);
        }
    }

    public static void cancel(LTRect ltRect, int uniqueId)
    {
        if (uniqueId >= 0)
        {
            init();
            int backId = uniqueId & 0xFFFF;
            int backCounter = uniqueId >> 16;
            // Debug.Log("uniqueId:"+uniqueId+ " id:"+backId +" action:"+(TweenAction)backType + " tweens[id].type:"+tweens[backId].type);
            if (tweens[backId].ltRect == ltRect && tweens[backId].counter == backCounter)
                removeTween((int)backId);
        }
    }

    /// <summary>
    /// Cancel a specific tween with the provided id
    /// <summary>
    /// <param name="id">unique id that represents that tween</param>
    /// <param name="callOnComplete">(optional) whether to call the onComplete method before canceling</param>
    public static void cancel(int uniqueId)
    {
        cancel(uniqueId, false);
    }
    public static void cancel(int uniqueId, bool callOnComplete)
    {
        if (uniqueId >= 0)
        {
            init();
            int backId = uniqueId & 0xFFFF;
            int backCounter = uniqueId >> 16;
            // Debug.Log("uniqueId:"+uniqueId+ " id:"+backId +" action:"+(TweenAction)backType + " tweens[id].type:"+tweens[backId].type);
            if (tweens[backId].counter == backCounter)
            {
                if (callOnComplete && tweens[backId].optional.onComplete != null)
                    tweens[backId].optional.onComplete();
                removeTween((int)backId);
            }
        }
    }

    /// <summary>
    /// Retrieve a tweens LTDescr object to modify
    /// <summary>
    /// <param name="id">unique id that represents that tween</param>
    public static LTDescr descr(int uniqueId)
    {
        int backId = uniqueId & 0xFFFF;
        int backCounter = uniqueId >> 16;

        if (tweens[backId] != null && tweens[backId].uniqueId == uniqueId && tweens[backId].counter == backCounter)
            return tweens[backId];
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].uniqueId == uniqueId && tweens[i].counter == backCounter)
                return tweens[i];
        }
        return null;
    }

    public static LTDescr description(int uniqueId)
    {
        return descr(uniqueId);
    }

    /// <summary>
    /// Retrieve a tweens LTDescr object(s) to modify
    /// <summary>
    /// <param name="id">object whose tween descriptions you want to retrieve</param>
    public static LTDescr[] descriptions(GameObject gameObject = null)
    {
        if (gameObject == null) return null;

        List<LTDescr> descrs = new List<LTDescr>();
        Transform trans = gameObject.transform;
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].toggle && tweens[i].trans == trans)
                descrs.Add(tweens[i]);
        }
        return descrs.ToArray();
    }

    [System.Obsolete("Use 'pause( id )' instead")]
    public static void pause(GameObject gameObject, int uniqueId)
    {
        pause(uniqueId);
    }

    /// <summary>
    /// Pause all tweens for a GameObject
    /// <summary>
    /// <param name="id">Id of the tween you want to pause</param>
    public static void pause(int uniqueId)
    {
        int backId = uniqueId & 0xFFFF;
        int backCounter = uniqueId >> 16;
        if (tweens[backId].counter == backCounter)
        {
            tweens[backId].pause();
        }
    }

    /// <summary>
    /// Pause all tweens for a GameObject
    /// <summary>
    /// <param name="gameObject">GameObject whose tweens you want to pause</param>
    public static void pause(GameObject gameObject)
    {
        Transform trans = gameObject.transform;
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].trans == trans)
            {
                tweens[i].pause();
            }
        }
    }

    /// <summary>
    /// Pause all active tweens
    /// <summary>
    public static void pauseAll()
    {
        init();
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            tweens[i].pause();
        }
    }

    /// <summary>
    /// Resume all active tweens
    /// <summary>
    public static void resumeAll()
    {
        init();
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            tweens[i].resume();
        }
    }

    [System.Obsolete("Use 'resume( id )' instead")]
    public static void resume(GameObject gameObject, int uniqueId)
    {
        resume(uniqueId);
    }

    /// <summary>
    /// Resume a specific tween
    /// <summary>
    /// <param name="id">Id of the tween you want to resume</param>
    public static void resume(int uniqueId)
    {
        int backId = uniqueId & 0xFFFF;
        int backCounter = uniqueId >> 16;
        if (tweens[backId].counter == backCounter)
        {
            tweens[backId].resume();
        }
    }

    /// <summary>
    /// Resume all the tweens on a GameObject
    /// <summary>
    /// <param name="gameObject">GameObject whose tweens you want to resume</param>
    public static void resume(GameObject gameObject)
    {
        Transform trans = gameObject.transform;
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].trans == trans)
                tweens[i].resume();
        }
    }

    /// <summary>
    /// Test whether or not a tween is active on a GameObject
    /// <summary>
    /// <param name="gameObject">GameObject that you want to test if it is tweening</param>
    public static bool isTweening(GameObject gameObject = null)
    {
        if (gameObject == null)
        {
            for (int i = 0; i <= tweenMaxSearch; i++)
            {
                if (tweens[i].toggle)
                    return true;
            }
            return false;
        }
        Transform trans = gameObject.transform;
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].toggle && tweens[i].trans == trans)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Test whether or not a tween is active or not
    /// <summary>
    /// <param name="id">id of the tween that you want to test if it is tweening</param>
    public static bool isTweening(int uniqueId)
    {
        int backId = uniqueId & 0xFFFF;
        int backCounter = uniqueId >> 16;
        if (backId < 0 || backId >= maxTweens) return false;
        // Debug.Log("tweens[backId].counter:"+tweens[backId].counter+" backCounter:"+backCounter +" toggle:"+tweens[backId].toggle);
        if (tweens[backId].counter == backCounter && tweens[backId].toggle)
        {
            return true;
        }
        return false;
    }

    public static bool isTweening(LTRect ltRect)
    {
        for (int i = 0; i <= tweenMaxSearch; i++)
        {
            if (tweens[i].toggle && tweens[i].ltRect == ltRect)
                return true;
        }
        return false;
    }

    public static void drawBezierPath(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float arrowSize = 0.0f, Transform arrowTransform = null)
    {
        Vector3 last = a;
        Vector3 p;
        Vector3 aa = (-a + 3 * (b - c) + d);
        Vector3 bb = 3 * (a + c) - 6 * b;
        Vector3 cc = 3 * (b - a);

        float t;

        if (arrowSize > 0.0f)
        {
            Vector3 beforePos = arrowTransform.position;
            Quaternion beforeQ = arrowTransform.rotation;
            float distanceTravelled = 0f;

            for (float k = 1.0f; k <= 120.0f; k++)
            {
                t = k / 120.0f;
                p = ((aa * t + (bb)) * t + cc) * t + a;
                Gizmos.DrawLine(last, p);
                distanceTravelled += (p - last).magnitude;
                if (distanceTravelled > 1f)
                {
                    distanceTravelled = distanceTravelled - 1f;

                    arrowTransform.position = p;
                    arrowTransform.LookAt(last, Vector3.forward);
                    Vector3 to = arrowTransform.TransformDirection(Vector3.right);
                    // Debug.Log("to:"+to+" tweenEmpty.transform.position:"+arrowTransform.position);
                    Vector3 back = (last - p);
                    back = back.normalized;
                    Gizmos.DrawLine(p, p + (to + back) * arrowSize);
                    to = arrowTransform.TransformDirection(-Vector3.right);
                    Gizmos.DrawLine(p, p + (to + back) * arrowSize);
                }
                last = p;
            }

            arrowTransform.position = beforePos;
            arrowTransform.rotation = beforeQ;
        }
        else
        {
            for (float k = 1.0f; k <= 30.0f; k++)
            {
                t = k / 30.0f;
                p = ((aa * t + (bb)) * t + cc) * t + a;
                Gizmos.DrawLine(last, p);
                last = p;
            }
        }
    }

    public static object logError(string error)
    {
        if (throwErrors) Debug.LogError(error); else Debug.Log(error);
        return null;
    }

    // LeanTween 2.0 Methods

    public static LTDescr options(LTDescr seed) { Debug.LogError("error this function is no longer used"); return null; }
    public static LTDescr options()
    {
        init();

        bool found = false;
        //		Debug.Log("Search start");
        for (j = 0, i = startSearch; j < maxTweens; i++)
        {
            if (i >= maxTweens - 1)
                i = 0;
            //			Debug.Log("searching i:"+i);
            if (tweens[i].toggle == false)
            {
                if (i + 1 > tweenMaxSearch)
                    tweenMaxSearch = i + 1;
                startSearch = i + 1;
                found = true;
                break;
            }

            j++;
            if (j >= maxTweens)
                return logError("LeanTween - You have run out of available spaces for tweening. To avoid this error increase the number of spaces to available for tweening when you initialize the LeanTween class ex: LeanTween.init( " + (maxTweens * 2) + " );") as LTDescr;
        }
        if (found == false)
            logError("no available tween found!");

        // Debug.Log("new tween with i:"+i+" counter:"+tweens[i].counter+" tweenMaxSearch:"+tweenMaxSearch+" tween:"+tweens[i]);
        tweens[i].reset();
        tweens[i].setId((uint)i);

        return tweens[i];
    }

    public static GameObject tweenEmpty
    {
        get
        {
            init(maxTweens);
            return _tweenEmpty;
        }
    }

    public static int startSearch = 0;
    public static LTDescr d;

    private static LTDescr pushNewTween(GameObject gameObject, Vector3 to, float time, LTDescr tween)
    {
        init(maxTweens);
        if (gameObject == null || tween == null)
            return null;

        tween.trans = gameObject.transform;
        tween.to = to;
        tween.time = time;
        //tween.hasPhysics = gameObject.rigidbody!=null;

        return tween;
    }

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5
    /// <summary>
    /// Play a sequence of images on a Unity UI Object
    /// <summary>
    /// <param name="rectTransform">RectTransform that you want to play the sequence of sprites on</param>
    /// <param name="sprites">Sequence of sprites to be played</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr play(RectTransform rectTransform, UnityEngine.Sprite[] sprites)
    {
        float defaultFrameRate = 0.25f;
        float time = defaultFrameRate * sprites.Length;
        return pushNewTween(rectTransform.gameObject, new Vector3((float)sprites.Length - 1.0f, 0, 0), time, options().setCanvasPlaySprite().setSprites(sprites).setRepeat(-1));
    }
#endif

    /// <summary>
    /// Fade a gameobject's material to a certain alpha value. The material's shader needs to support alpha. <a href="http://owlchemylabs.com/content/">Owl labs has some excellent efficient shaders</a>.
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to fade</param>
    /// <param name="to">the final alpha value (0-1)</param>
    /// <param name="time">The time with which to fade the object</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr alpha(GameObject gameObject, float to, float time)
    {
        LTDescr lt = pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setAlpha());

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2
        SpriteRenderer ren = gameObject.GetComponent<SpriteRenderer>();
        lt.spriteRen = ren;
#endif
        return lt;
    }

    /// <summary>
    /// Fade a GUI Object
    /// <summary>
    /// <param name="ltRect">LTRect that you wish to fade</param>
    /// <param name="to">the final alpha value (0-1)</param>
    /// <param name="time">The time with which to fade the object</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr alpha(LTRect ltRect, float to, float time)
    {
        ltRect.alphaEnabled = true;
        return pushNewTween(tweenEmpty, new Vector3(to, 0f, 0f), time, options().setGUIAlpha().setRect(ltRect));
    }


#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5
    /// <summary>
    /// Fade a Unity UI Object
    /// <summary>
    /// <param name="rectTransform">RectTransform associated with the Text Component you wish to fade</param>
    /// <param name="to">the final alpha value (0-1)</param>
    /// <param name="time">The time with which to fade the object</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr textAlpha(RectTransform rectTransform, float to, float time)
    {
        return pushNewTween(rectTransform.gameObject, new Vector3(to, 0, 0), time, options().setTextAlpha());
    }
    public static LTDescr alphaText(RectTransform rectTransform, float to, float time)
    {
        return pushNewTween(rectTransform.gameObject, new Vector3(to, 0, 0), time, options().setTextAlpha());
    }

    /// <summary>
    /// Fade a Unity UI Canvas Group
    /// <summary>
    /// <param name="rectTransform">RectTransform that the CanvasGroup is attached to</param>
    /// <param name="to">the final alpha value (0-1)</param>
    /// <param name="time">The time with which to fade the object</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr alphaCanvas(CanvasGroup canvasGroup, float to, float time)
    {
        return pushNewTween(canvasGroup.gameObject, new Vector3(to, 0, 0), time, options().setCanvasGroupAlpha());
    }
#endif

    /// <summary>
    /// This works by tweening the vertex colors directly.<br>
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to alpha</param>
    /// <param name="to">The alpha value you wish to tween to</param>
    /// <param name="time">The time with which to delay before calling the function</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr alphaVertex(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, options().setAlphaVertex());
    }

    /// <summary>
    /// Change a gameobject's material to a certain color value. The material's shader needs to support color tinting. <a href="http://owlchemylabs.com/content/">Owl labs has some excellent efficient shaders</a>.
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to change the color</param>
    /// <param name="to">the final color value ex: Color.Red, new Color(1.0f,1.0f,0.0f,0.8f)</param>
    /// <param name="time">The time with which to fade the object</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr color(GameObject gameObject, Color to, float time)
    {
        LTDescr lt = pushNewTween(gameObject, new Vector3(1.0f, to.a, 0.0f), time, options().setColor().setPoint(new Vector3(to.r, to.g, to.b)));
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2
        SpriteRenderer ren = gameObject.GetComponent<SpriteRenderer>();
        lt.spriteRen = ren;
#endif
        return lt;
    }

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5
    /// <summary>
    /// Change the color a Unity UI Object
    /// <summary>
    /// <param name="rectTransform">RectTransform attached to the Text Component whose color you want to change</param>
    /// <param name="to">the final alpha value ex: Color.Red, new Color(1.0f,1.0f,0.0f,0.8f)</param>
    /// <param name="time">The time with which to fade the object</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr textColor(RectTransform rectTransform, Color to, float time)
    {
        return pushNewTween(rectTransform.gameObject, new Vector3(1.0f, to.a, 0.0f), time, options().setTextColor().setPoint(new Vector3(to.r, to.g, to.b)));
    }
    public static LTDescr colorText(RectTransform rectTransform, Color to, float time)
    {
        return pushNewTween(rectTransform.gameObject, new Vector3(1.0f, to.a, 0.0f), time, options().setTextColor().setPoint(new Vector3(to.r, to.g, to.b)));
    }
#endif

    /// <summary>
    /// Call a method after a specified amount of time
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to associate with this delayed call</param>
    /// <param name="time">delay The time you wish to pass before the method is called</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr delayedCall(float delayTime, Action callback)
    {
        return pushNewTween(tweenEmpty, Vector3.zero, delayTime, options().setCallback().setOnComplete(callback));
    }

    public static LTDescr delayedCall(float delayTime, Action<object> callback)
    {
        return pushNewTween(tweenEmpty, Vector3.zero, delayTime, options().setCallback().setOnComplete(callback));
    }

    public static LTDescr delayedCall(GameObject gameObject, float delayTime, Action callback)
    {
        return pushNewTween(gameObject, Vector3.zero, delayTime, options().setCallback().setOnComplete(callback));
    }

    public static LTDescr delayedCall(GameObject gameObject, float delayTime, Action<object> callback)
    {
        return pushNewTween(gameObject, Vector3.zero, delayTime, options().setCallback().setOnComplete(callback));
    }

    public static LTDescr destroyAfter(LTRect rect, float delayTime)
    {
        return pushNewTween(tweenEmpty, Vector3.zero, delayTime, options().setCallback().setRect(rect).setDestroyOnComplete(true));
    }


    /// <summary>
    /// Move a GameObject to a certain location
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to move</param>
    /// <param name="vec">to The final positin with which to move to</param>
    /// <param name="time">time The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr move(GameObject gameObject, Vector3 to, float time)
    {
        return pushNewTween(gameObject, to, time, options().setMove());
    }
    public static LTDescr move(GameObject gameObject, Vector2 to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to.x, to.y, gameObject.transform.position.z), time, options().setMove());
    }


    /// <summary>
    /// Move a GameObject along a set of bezier curves
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to move</param>
    /// <param name="path">A set of points that define the curve(s) ex: Point1,Handle2,Handle1,Point2,...</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr move(GameObject gameObject, Vector3[] to, float time)
    {
        d = options().setMoveCurved();
        if (d.optional.path == null)
            d.optional.path = new LTBezierPath(to);
        else
            d.optional.path.setPoints(to);

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    public static LTDescr move(GameObject gameObject, LTBezierPath to, float time)
    {
        d = options().setMoveCurved();
        d.optional.path = to;

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    public static LTDescr move(GameObject gameObject, LTSpline to, float time)
    {
        d = options().setMoveSpline();
        d.optional.spline = to;

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    /// <summary>
    /// Move a GameObject through a set of points
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to move</param>
    /// <param name="path">A set of points that define the curve(s) ex: ControlStart,Pt1,Pt2,Pt3,.. ..ControlEnd<br>Note: The first and last item just define the angle of the end points, they are not actually used in the spline path itself. If you do not care about the angle you can jus set the first two items and last two items as the same value.</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveSpline(GameObject gameObject, Vector3[] to, float time)
    {
        d = options().setMoveSpline();
        d.optional.spline = new LTSpline(to);

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    /// <summary>
    /// Move a GameObject through a set of points
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to move</param>
    /// <param name="spline">pass a pre-existing LTSpline for the object to move along</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveSpline(GameObject gameObject, LTSpline to, float time)
    {
        d = options().setMoveSpline();
        d.optional.spline = to;

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    /// <summary>
    /// Move a GameObject through a set of points, in local space
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to move</param>
    /// <param name="path">A set of points that define the curve(s) ex: ControlStart,Pt1,Pt2,Pt3,.. ..ControlEnd</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveSplineLocal(GameObject gameObject, Vector3[] to, float time)
    {
        d = options().setMoveSplineLocal();
        d.optional.spline = new LTSpline(to);

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    /// <summary>
    /// Move a GUI Element to a certain location
    /// <summary>
    /// <param name="ltRect">ltRect LTRect object that you wish to move</param>
    /// <param name="vec">to The final position with which to move to (pixel coordinates)</param>
    /// <param name="time">time The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr move(LTRect ltRect, Vector2 to, float time)
    {
        return pushNewTween(tweenEmpty, to, time, options().setGUIMove().setRect(ltRect));
    }

    public static LTDescr moveMargin(LTRect ltRect, Vector2 to, float time)
    {
        return pushNewTween(tweenEmpty, to, time, options().setGUIMoveMargin().setRect(ltRect));
    }

    /// <summary>
    /// Move a GameObject along the x-axis
    /// <summary>
    /// <param name="gameObject">gameObject Gameobject that you wish to move</param>
    /// <param name="to">to The final position with which to move to</param>
    /// <param name="time">time The time to complete the move in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveX(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setMoveX());
    }

    /// <summary>
    /// Move a GameObject along the y-axis
    /// <summary>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveY(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setMoveY());
    }

    /// <summary>
    /// Move a GameObject along the z-axis
    /// <summary>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveZ(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setMoveZ());
    }

    /// <summary>
    /// Move a GameObject to a certain location relative to the parent transform.
    /// <summary>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveLocal(GameObject gameObject, Vector3 to, float time)
    {
        return pushNewTween(gameObject, to, time, options().setMoveLocal());
    }

    /// <summary>
    /// Move a GameObject along a set of bezier curves, in local space
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to move</param>
    /// <param name="path">A set of points that define the curve(s) ex: Point1,Handle1,Handle2,Point2,...</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveLocal(GameObject gameObject, Vector3[] to, float time)
    {
        d = options().setMoveCurvedLocal();
        if (d.optional.path == null)
            d.optional.path = new LTBezierPath(to);
        else
            d.optional.path.setPoints(to);

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    public static LTDescr moveLocalX(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setMoveLocalX());
    }

    public static LTDescr moveLocalY(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setMoveLocalY());
    }

    public static LTDescr moveLocalZ(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setMoveLocalZ());
    }

    public static LTDescr moveLocal(GameObject gameObject, LTBezierPath to, float time)
    {
        d = options().setMoveCurvedLocal();
        d.optional.path = to;

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }
    public static LTDescr moveLocal(GameObject gameObject, LTSpline to, float time)
    {
        d = options().setMoveSplineLocal();
        d.optional.spline = to;

        return pushNewTween(gameObject, new Vector3(1.0f, 0.0f, 0.0f), time, d);
    }

    /// <summary>
    /// Move a GameObject to another transform
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to move</param>
    /// <param name="destination">Transform whose position the tween will finally end on</param>
    /// <param name="time">time The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr move(GameObject gameObject, Transform to, float time)
    {
        return pushNewTween(gameObject, Vector3.zero, time, options().setTo(to).setMoveToTransform());
    }

    /// <summary>
    /// Rotate a GameObject, to values are in passed in degrees
    /// <summary>
    /// <returns>LTDescr an object that distinguishes the tween</returns>

    public static LTDescr rotate(GameObject gameObject, Vector3 to, float time)
    {
        return pushNewTween(gameObject, to, time, options().setRotate());
    }

    /// <summary>
    /// Rotate a GUI element (using an LTRect object), to a value that is in degrees
    /// <summary>
    /// <param name="ltRect">LTRect that you wish to rotate</param>
    /// <param name="to">The final rotation with which to rotate to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <param name="optional">Object Array where you can pass <a href="#optional">optional items</a>.</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotate(LTRect ltRect, float to, float time)
    {
        return pushNewTween(tweenEmpty, new Vector3(to, 0f, 0f), time, options().setGUIRotate().setRect(ltRect));
    }

    /// <summary>
    /// Rotate a GameObject in the objects local space (on the transforms localEulerAngles object)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to rotate</param>
    /// <param name="to">The final rotation with which to rotate to</param>
    /// <param name="time">The time to complete the rotation in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateLocal(GameObject gameObject, Vector3 to, float time)
    {
        return pushNewTween(gameObject, to, time, options().setRotateLocal());
    }

    /// <summary>
    /// Rotate a GameObject only on the X axis
    /// <summary>
    /// <param name="to">The final x-axis rotation with which to rotate</param>
    /// <param name="time">The time to complete the rotation in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateX(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setRotateX());
    }

    /// <summary>
    /// Rotate a GameObject only on the Y axis
    /// <summary>
    /// <param name="to">The final y-axis rotation with which to rotate</param>
    /// <param name="time">The time to complete the rotation in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateY(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setRotateY());
    }

    /// <summary>
    /// Rotate a GameObject only on the Z axis
    /// <summary>
    /// <param name="to">The final z-axis rotation with which to rotate</param>
    /// <param name="time">The time to complete the rotation in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateZ(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setRotateZ());
    }

    /// <summary>
    /// Rotate a GameObject around a certain Axis (the best method to use when you want to rotate beyond 180 degrees)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to rotate</param>
    /// <param name="vec">axis in which to rotate around ex: Vector3.up</param>
    /// <param name="degrees">the degrees in which to rotate</param>
    /// <param name="time">time The time to complete the rotation in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateAround(GameObject gameObject, Vector3 axis, float add, float time)
    {
        return pushNewTween(gameObject, new Vector3(add, 0f, 0f), time, options().setAxis(axis).setRotateAround());
    }

    /// <summary>
    /// Rotate a GameObject around a certain Axis in Local Space (the best method to use when you want to rotate beyond 180 degrees)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to rotate</param>
    /// <param name="vec">axis in which to rotate around ex: Vector3.up</param>
    /// <param name="degrees">the degrees in which to rotate</param>
    /// <param name="time">time The time to complete the rotation in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateAroundLocal(GameObject gameObject, Vector3 axis, float add, float time)
    {
        return pushNewTween(gameObject, new Vector3(add, 0f, 0f), time, options().setRotateAroundLocal().setAxis(axis));
    }

    /// <summary>
    /// Scale a GameObject to a certain size
    /// <summary>
    /// <param name="gameObject">gameObject Gameobject that you wish to scale</param>
    /// <param name="vec">to The size with which to tween to</param>
    /// <param name="time">time The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr scale(GameObject gameObject, Vector3 to, float time)
    {
        return pushNewTween(gameObject, to, time, options().setScale());
    }

    /// <summary>
    /// Scale a GUI Element to a certain width and height
    /// <summary>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr scale(LTRect ltRect, Vector2 to, float time)
    {
        return pushNewTween(tweenEmpty, to, time, options().setGUIScale().setRect(ltRect));
    }

    /// <summary>
    /// Scale a GameObject to a certain size along the x-axis only
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to scale</param>
    /// <param name="scaleTo">the size with which to scale to</param>
    /// <param name="time">the time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr scaleX(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setScaleX());
    }

    /// <summary>
    /// Scale a GameObject to a certain size along the y-axis only
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to scale</param>
    /// <param name="scaleTo">the size with which to scale to</param>
    /// <param name="time">the time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr scaleY(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setScaleY());
    }

    /// <summary>
    /// Scale a GameObject to a certain size along the z-axis only
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to scale</param>
    /// <param name="scaleTo">the size with which to scale to</param>
    /// <param name="time">the time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr scaleZ(GameObject gameObject, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setScaleZ());
    }

    /// <summary>
    /// Tween any particular value (float)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to attach the tween to</param>
    /// <param name="from">The original value to start the tween from</param>
    /// <param name="to">The final float with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr value(GameObject gameObject, float from, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setCallback().setFrom(new Vector3(from, 0, 0)));
    }
    public static LTDescr value(float from, float to, float time)
    {
        return pushNewTween(tweenEmpty, new Vector3(to, 0, 0), time, options().setCallback().setFrom(new Vector3(from, 0, 0)));
    }

    /// <summary>
    /// Tween any particular value (Vector2)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to attach the tween to</param>
    /// <param name="from">The original value to start the tween from</param>
    /// <param name="to">The final Vector2 with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr value(GameObject gameObject, Vector2 from, Vector2 to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to.x, to.y, 0), time, options().setValue3().setTo(new Vector3(to.x, to.y, 0f)).setFrom(new Vector3(from.x, from.y, 0)));
    }

    /// <summary>
    /// Tween any particular value (Vector3)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to attach the tween to</param>
    /// <param name="from">The original value to start the tween from</param>
    /// <param name="to">The final Vector3 with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr value(GameObject gameObject, Vector3 from, Vector3 to, float time)
    {
        return pushNewTween(gameObject, to, time, options().setValue3().setFrom(from));
    }

    /// <summary>
    /// Tween any particular value (Color)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to attach the tween to</param>
    /// <param name="from">The original value to start the tween from</param>
    /// <param name="to">The final Color with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr value(GameObject gameObject, Color from, Color to, float time)
    {
        LTDescr lt = pushNewTween(gameObject, new Vector3(1f, to.a, 0f), time, options().setCallbackColor().setPoint(new Vector3(to.r, to.g, to.b))
        .setFromColor(from).setHasInitialized(false));

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2
        SpriteRenderer ren = gameObject.GetComponent<SpriteRenderer>();
        lt.spriteRen = ren;
#endif
        return lt;
    }

    /// <summary>
    /// Tween any particular value, it does not need to be tied to any particular type or GameObject
    /// <summary>
    /// <param name="callOnUpdate">The function that is called on every Update frame, this function needs to accept a float value ex: function updateValue( float val ){ }</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>

    public static LTDescr value(GameObject gameObject, Action<float> callOnUpdate, float from, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setCallback().setTo(new Vector3(to, 0, 0)).setFrom(new Vector3(from, 0, 0)).setOnUpdate(callOnUpdate));
    }

    /// <summary>
    /// Tweens any float value, it does not need to be tied to any particular type or GameObject
    /// <summary>
    /// <returns>LTDescr an object that distinguishes the tween</returns>

    public static LTDescr value(GameObject gameObject, Action<float, float> callOnUpdateRatio, float from, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setCallback().setTo(new Vector3(to, 0, 0)).setFrom(new Vector3(from, 0, 0)).setOnUpdateRatio(callOnUpdateRatio));
    }

    /// <summary>
    /// Tween from one color to another
    /// <summary>
    /// <param name="callOnUpdate">The function that is called on every Update frame, this function needs to accept a color value ex: function updateValue( Color val ){ }</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>

    public static LTDescr value(GameObject gameObject, Action<Color> callOnUpdate, Color from, Color to, float time)
    {
        return pushNewTween(gameObject, new Vector3(1.0f, to.a, 0.0f), time, options().setCallbackColor().setPoint(new Vector3(to.r, to.g, to.b))
        .setAxis(new Vector3(from.r, from.g, from.b)).setFrom(new Vector3(0.0f, from.a, 0.0f)).setHasInitialized(false).setOnUpdateColor(callOnUpdate));
    }
    public static LTDescr value(GameObject gameObject, Action<Color, object> callOnUpdate, Color from, Color to, float time)
    {
        return pushNewTween(gameObject, new Vector3(1.0f, to.a, 0.0f), time, options().setCallbackColor().setPoint(new Vector3(to.r, to.g, to.b))
        .setAxis(new Vector3(from.r, from.g, from.b)).setFrom(new Vector3(0.0f, from.a, 0.0f)).setHasInitialized(false).setOnUpdateColor(callOnUpdate));
    }

    /// <summary>
    /// Tween any particular value (Vector2), this could be used to tween an arbitrary value like offset property
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to attach the tween to</param>
    /// <param name="callOnUpdate">The function that is called on every Update frame, this function needs to accept a float value ex: function updateValue( Vector3 val ){ }</param>
    /// <param name="from">The original value to start the tween from</param>
    /// <param name="to">The final Vector3 with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr value(GameObject gameObject, Action<Vector2> callOnUpdate, Vector2 from, Vector2 to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to.x, to.y, 0f), time, options().setValue3().setTo(new Vector3(to.x, to.y, 0f)).setFrom(new Vector3(from.x, from.y, 0f)).setOnUpdateVector2(callOnUpdate));
    }

    /// <summary>
    /// Tween any particular value (Vector3), this could be used to tween an arbitrary property that uses a Vector
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to attach the tween to</param>
    /// <param name="callOnUpdate">The function that is called on every Update frame, this function needs to accept a float value ex: function updateValue( Vector3 val ){ }</param>
    /// <param name="from">The original value to start the tween from</param>
    /// <param name="to">The final Vector3 with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr value(GameObject gameObject, Action<Vector3> callOnUpdate, Vector3 from, Vector3 to, float time)
    {
        return pushNewTween(gameObject, to, time, options().setValue3().setTo(to).setFrom(from).setOnUpdateVector3(callOnUpdate));
    }

    /// <summary>
    /// Tween any particular value (float)
    /// <summary>
    /// <param name="gameObject">Gameobject that you wish to attach the tween to</param>
    /// <param name="callOnUpdate">The function that is called on every Update frame, this function needs to accept a float value ex: function updateValue( Vector3 val, object obj ){ }</param>
    /// <param name="from">The original value to start the tween from</param>
    /// <param name="to">The final Vector3 with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr value(GameObject gameObject, Action<float, object> callOnUpdate, float from, float to, float time)
    {
        return pushNewTween(gameObject, new Vector3(to, 0, 0), time, options().setCallback().setTo(new Vector3(to, 0, 0)).setFrom(new Vector3(from, 0, 0)).setOnUpdate(callOnUpdate, gameObject));
    }

    public static LTDescr delayedSound(AudioClip audio, Vector3 pos, float volume)
    {
        //Debug.LogError("Delay sound??");
        return pushNewTween(tweenEmpty, pos, 0f, options().setDelayedSound().setTo(pos).setFrom(new Vector3(volume, 0, 0)).setAudio(audio));
    }

    public static LTDescr delayedSound(GameObject gameObject, AudioClip audio, Vector3 pos, float volume)
    {
        //Debug.LogError("Delay sound??");
        return pushNewTween(gameObject, pos, 0f, options().setDelayedSound().setTo(pos).setFrom(new Vector3(volume, 0, 0)).setAudio(audio));
    }

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5

    /// <summary>
    /// Move a RectTransform object (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final Vector3 with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr move(RectTransform rectTrans, Vector3 to, float time)
    {
        return pushNewTween(rectTrans.gameObject, to, time, options().setCanvasMove().setRect(rectTrans));
    }

    /// <summary>
    /// Move a RectTransform object affecting x-axis only (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final x location with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveX(RectTransform rectTrans, float to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(to, 0f, 0f), time, options().setCanvasMoveX().setRect(rectTrans));
    }

    /// <summary>
    /// Move a RectTransform object affecting y-axis only (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final y location with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveY(RectTransform rectTrans, float to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(to, 0f, 0f), time, options().setCanvasMoveY().setRect(rectTrans));
    }

    /// <summary>
    /// Move a RectTransform object affecting z-axis only (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final x location with which to tween to</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr moveZ(RectTransform rectTrans, float to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(to, 0f, 0f), time, options().setCanvasMoveZ().setRect(rectTrans));
    }

    /// <summary>
    /// Rotate a RectTransform object (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The degree with which to rotate the RectTransform</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotate(RectTransform rectTrans, float to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(to, 0f, 0f), time, options().setCanvasRotateAround().setRect(rectTrans).setAxis(Vector3.forward));
    }

    public static LTDescr rotate(RectTransform rectTrans, Vector3 to, float time)
    {
        return pushNewTween(rectTrans.gameObject, to, time, options().setCanvasRotateAround().setRect(rectTrans).setAxis(Vector3.forward));
    }

    /// <summary>
    /// Rotate a RectTransform object (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="axis">The axis in which to rotate the RectTransform (Vector3.forward is most commonly used)</param>
    /// <param name="to">The degree with which to rotate the RectTransform</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateAround(RectTransform rectTrans, Vector3 axis, float to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(to, 0f, 0f), time, options().setCanvasRotateAround().setRect(rectTrans).setAxis(axis));
    }

    /// <summary>
    /// Rotate a RectTransform object around it's local axis (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="axis">The local axis in which to rotate the RectTransform (Vector3.forward is most commonly used)</param>
    /// <param name="to">The degree with which to rotate the RectTransform</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr rotateAroundLocal(RectTransform rectTrans, Vector3 axis, float to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(to, 0f, 0f), time, options().setCanvasRotateAroundLocal().setRect(rectTrans).setAxis(axis));
    }

    /// <summary>
    /// Scale a RectTransform object (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final Vector3 with which to tween to (localScale)</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr scale(RectTransform rectTrans, Vector3 to, float time)
    {
        return pushNewTween(rectTrans.gameObject, to, time, options().setCanvasScale().setRect(rectTrans));
    }

    /// <summary>
    /// Change the sizeDelta of a RectTransform object (used in Unity Canvas, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final Vector2 the tween will end at for sizeDelta property</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr size(RectTransform rectTrans, Vector2 to, float time)
    {
        return pushNewTween(rectTrans.gameObject, to, time, options().setCanvasSizeDelta().setRect(rectTrans));
    }

    /// <summary>
    /// Alpha an Image Component attached to a RectTransform (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final Vector3 with which to tween to (localScale)</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr alpha(RectTransform rectTrans, float to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(to, 0f, 0f), time, options().setCanvasAlpha().setRect(rectTrans));
    }

    /// <summary>
    /// Change the Color of an Image Component attached to a RectTransform (used in Unity GUI in 4.6+, for Buttons, Panel, Scrollbar, etc...)
    /// <summary>
    /// <param name="rectTrans">RectTransform that you wish to attach the tween to</param>
    /// <param name="to">The final Vector3 with which to tween to (localScale)</param>
    /// <param name="time">The time to complete the tween in</param>
    /// <returns>LTDescr an object that distinguishes the tween</returns>
    public static LTDescr color(RectTransform rectTrans, Color to, float time)
    {
        return pushNewTween(rectTrans.gameObject, new Vector3(1.0f, to.a, 0.0f), time, options().setCanvasColor().setRect(rectTrans).setPoint(new Vector3(to.r, to.g, to.b)));
    }

#endif

    // Tweening Functions - Thanks to Robert Penner and GFX47

    public static float tweenOnCurve(LTDescr tweenDescr, float ratioPassed)
    {
        // Debug.Log("single ratio:"+ratioPassed+" tweenDescr.animationCurve.Evaluate(ratioPassed):"+tweenDescr.animationCurve.Evaluate(ratioPassed));
        return tweenDescr.from.x + (tweenDescr.diff.x) * tweenDescr.optional.animationCurve.Evaluate(ratioPassed);
    }

    public static Vector3 tweenOnCurveVector(LTDescr tweenDescr, float ratioPassed)
    {
        return new Vector3(tweenDescr.from.x + (tweenDescr.diff.x) * tweenDescr.optional.animationCurve.Evaluate(ratioPassed),
        tweenDescr.from.y + (tweenDescr.diff.y) * tweenDescr.optional.animationCurve.Evaluate(ratioPassed),
        tweenDescr.from.z + (tweenDescr.diff.z) * tweenDescr.optional.animationCurve.Evaluate(ratioPassed));
    }

    public static float easeOutQuadOpt(float start, float diff, float ratioPassed)
    {
        return -diff * ratioPassed * (ratioPassed - 2) + start;
    }

    public static float easeInQuadOpt(float start, float diff, float ratioPassed)
    {
        return diff * ratioPassed * ratioPassed + start;
    }

    public static float easeInOutQuadOpt(float start, float diff, float ratioPassed)
    {
        ratioPassed /= .5f;
        if (ratioPassed < 1) return diff / 2 * ratioPassed * ratioPassed + start;
        ratioPassed--;
        return -diff / 2 * (ratioPassed * (ratioPassed - 2) - 1) + start;
    }

    public static Vector3 easeInOutQuadOpt(Vector3 start, Vector3 diff, float ratioPassed)
    {
        ratioPassed /= .5f;
        if (ratioPassed < 1) return diff / 2 * ratioPassed * ratioPassed + start;
        ratioPassed--;
        return -diff / 2 * (ratioPassed * (ratioPassed - 2) - 1) + start;
    }

    public static float linear(float start, float end, float val)
    {
        return Mathf.Lerp(start, end, val);
    }

    public static float clerp(float start, float end, float val)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) / 2.0f);
        float retval = 0.0f;
        float diff = 0.0f;
        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * val;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * val;
            retval = start + diff;
        }
        else retval = start + (end - start) * val;
        return retval;
    }

    public static float spring(float start, float end, float val)
    {
        val = Mathf.Clamp01(val);
        val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
        return start + (end - start) * val;
    }

    public static float easeInQuad(float start, float end, float val)
    {
        end -= start;
        return end * val * val + start;
    }

    public static float easeOutQuad(float start, float end, float val)
    {
        end -= start;
        return -end * val * (val - 2) + start;
    }

    public static float easeInOutQuad(float start, float end, float val)
    {
        val /= .5f;
        end -= start;
        if (val < 1) return end / 2 * val * val + start;
        val--;
        return -end / 2 * (val * (val - 2) - 1) + start;
    }


    public static float easeInOutQuadOpt2(float start, float diffBy2, float val, float val2)
    {
        val /= .5f;
        if (val < 1) return diffBy2 * val2 + start;
        val--;
        return -diffBy2 * ((val2 - 2) - 1f) + start;
    }

    public static float easeInCubic(float start, float end, float val)
    {
        end -= start;
        return end * val * val * val + start;
    }

    public static float easeOutCubic(float start, float end, float val)
    {
        val--;
        end -= start;
        return end * (val * val * val + 1) + start;
    }

    public static float easeInOutCubic(float start, float end, float val)
    {
        val /= .5f;
        end -= start;
        if (val < 1) return end / 2 * val * val * val + start;
        val -= 2;
        return end / 2 * (val * val * val + 2) + start;
    }

    public static float easeInQuart(float start, float end, float val)
    {
        end -= start;
        return end * val * val * val * val + start;
    }

    public static float easeOutQuart(float start, float end, float val)
    {
        val--;
        end -= start;
        return -end * (val * val * val * val - 1) + start;
    }

    public static float easeInOutQuart(float start, float end, float val)
    {
        val /= .5f;
        end -= start;
        if (val < 1) return end / 2 * val * val * val * val + start;
        val -= 2;
        return -end / 2 * (val * val * val * val - 2) + start;
    }

    public static float easeInQuint(float start, float end, float val)
    {
        end -= start;
        return end * val * val * val * val * val + start;
    }

    public static float easeOutQuint(float start, float end, float val)
    {
        val--;
        end -= start;
        return end * (val * val * val * val * val + 1) + start;
    }

    public static float easeInOutQuint(float start, float end, float val)
    {
        val /= .5f;
        end -= start;
        if (val < 1) return end / 2 * val * val * val * val * val + start;
        val -= 2;
        return end / 2 * (val * val * val * val * val + 2) + start;
    }

    public static float easeInSine(float start, float end, float val)
    {
        end -= start;
        return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
    }

    public static float easeOutSine(float start, float end, float val)
    {
        end -= start;
        return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
    }

    public static float easeInOutSine(float start, float end, float val)
    {
        end -= start;
        return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
    }

    public static float easeInExpo(float start, float end, float val)
    {
        end -= start;
        return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
    }

    public static float easeOutExpo(float start, float end, float val)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
    }

    public static float easeInOutExpo(float start, float end, float val)
    {
        val /= .5f;
        end -= start;
        if (val < 1) return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
        val--;
        return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
    }

    public static float easeInCirc(float start, float end, float val)
    {
        end -= start;
        return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
    }

    public static float easeOutCirc(float start, float end, float val)
    {
        val--;
        end -= start;
        return end * Mathf.Sqrt(1 - val * val) + start;
    }

    public static float easeInOutCirc(float start, float end, float val)
    {
        val /= .5f;
        end -= start;
        if (val < 1) return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
        val -= 2;
        return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
    }

    public static float easeInBounce(float start, float end, float val)
    {
        end -= start;
        float d = 1f;
        return end - easeOutBounce(0, end, d - val) + start;
    }

    public static float easeOutBounce(float start, float end, float val)
    {
        val /= 1f;
        end -= start;
        if (val < (1 / 2.75f))
        {
            return end * (7.5625f * val * val) + start;
        }
        else if (val < (2 / 2.75f))
        {
            val -= (1.5f / 2.75f);
            return end * (7.5625f * (val) * val + .75f) + start;
        }
        else if (val < (2.5 / 2.75))
        {
            val -= (2.25f / 2.75f);
            return end * (7.5625f * (val) * val + .9375f) + start;
        }
        else
        {
            val -= (2.625f / 2.75f);
            return end * (7.5625f * (val) * val + .984375f) + start;
        }
    }

    public static float easeInOutBounce(float start, float end, float val)
    {
        end -= start;
        float d = 1f;
        if (val < d / 2) return easeInBounce(0, end, val * 2) * 0.5f + start;
        else return easeOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
    }

    public static float easeInBack(float start, float end, float val, float overshoot = 1.0f)
    {
        end -= start;
        val /= 1;
        float s = 1.70158f * overshoot;
        return end * (val) * val * ((s + 1) * val - s) + start;
    }

    public static float easeOutBack(float start, float end, float val, float overshoot = 1.0f)
    {
        float s = 1.70158f * overshoot;
        end -= start;
        val = (val / 1) - 1;
        return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
    }

    public static float easeInOutBack(float start, float end, float val, float overshoot = 1.0f)
    {
        float s = 1.70158f * overshoot;
        end -= start;
        val /= .5f;
        if ((val) < 1)
        {
            s *= (1.525f) * overshoot;
            return end / 2 * (val * val * (((s) + 1) * val - s)) + start;
        }
        val -= 2;
        s *= (1.525f) * overshoot;
        return end / 2 * ((val) * val * (((s) + 1) * val + s) + 2) + start;
    }

    public static float easeInElastic(float start, float end, float val, float overshoot = 1.0f, float period = 0.3f)
    {
        end -= start;

        float p = period;
        float s = 0f;
        float a = 0f;

        if (val == 0f) return start;

        if (val == 1f) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4f;
        }
        else
        {
            s = p / (2f * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (overshoot > 1f && val > 0.6f)
            overshoot = 1f + ((1f - val) / 0.4f * (overshoot - 1f));
        // Debug.Log("ease in elastic val:"+val+" a:"+a+" overshoot:"+overshoot);

        val = val - 1f;
        return start - (a * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p)) * overshoot;
    }

    public static float easeOutElastic(float start, float end, float val, float overshoot = 1.0f, float period = 0.3f)
    {
        end -= start;

        float p = period;
        float s = 0f;
        float a = 0f;

        if (val == 0f) return start;

        // Debug.Log("ease out elastic val:"+val+" a:"+a);
        if (val == 1f) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4f;
        }
        else
        {
            s = p / (2f * Mathf.PI) * Mathf.Asin(end / a);
        }
        if (overshoot > 1f && val < 0.4f)
            overshoot = 1f + (val / 0.4f * (overshoot - 1f));
        // Debug.Log("ease out elastic val:"+val+" a:"+a+" overshoot:"+overshoot);

        return start + end + a * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p) * overshoot;
    }

    public static float easeInOutElastic(float start, float end, float val, float overshoot = 1.0f, float period = 0.3f)
    {
        end -= start;

        float p = period;
        float s = 0f;
        float a = 0f;

        if (val == 0f) return start;

        val = val / (1f / 2f);
        if (val == 2f) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4f;
        }
        else
        {
            s = p / (2f * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (overshoot > 1f)
        {
            if (val < 0.2f)
            {
                overshoot = 1f + (val / 0.2f * (overshoot - 1f));
            }
            else if (val > 0.8f)
            {
                overshoot = 1f + ((1f - val) / 0.2f * (overshoot - 1f));
            }
        }

        if (val < 1f)
        {
            val = val - 1f;
            return start - 0.5f * (a * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p)) * overshoot;
        }
        val = val - 1f;
        return end + start + a * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p) * 0.5f * overshoot;
    }

    // LeanTween Listening/Dispatch

    private static System.Action<LTEvent>[] eventListeners;
    private static GameObject[] goListeners;
    private static int eventsMaxSearch = 0;
    public static int EVENTS_MAX = 10;
    public static int LISTENERS_MAX = 10;
    private static int INIT_LISTENERS_MAX = LISTENERS_MAX;

    public static void addListener(int eventId, System.Action<LTEvent> callback)
    {
        addListener(tweenEmpty, eventId, callback);
    }

    /// <summary>
    /// Add a listener method to be called when the appropriate LeanTween.dispatchEvent is called
    /// <summary>
    /// <param name="caller">the gameObject the listener is attached to</param>
    /// <param name="eventId">a unique int that describes the event (best to use an enum)</param>
    /// <param name="callback">the method to call when the event has been dispatched</param>
    public static void addListener(GameObject caller, int eventId, System.Action<LTEvent> callback)
    {
        if (eventListeners == null)
        {
            INIT_LISTENERS_MAX = LISTENERS_MAX;
            eventListeners = new System.Action<LTEvent>[EVENTS_MAX * LISTENERS_MAX];
            goListeners = new GameObject[EVENTS_MAX * LISTENERS_MAX];
        }
        // Debug.Log("searching for an empty space for:"+caller + " eventid:"+event);
        for (i = 0; i < INIT_LISTENERS_MAX; i++)
        {
            int point = eventId * INIT_LISTENERS_MAX + i;
            if (goListeners[point] == null || eventListeners[point] == null)
            {
                eventListeners[point] = callback;
                goListeners[point] = caller;
                if (i >= eventsMaxSearch)
                    eventsMaxSearch = i + 1;
                // Debug.Log("adding event for:"+caller.name);

                return;
            }
#if UNITY_FLASH
            if(goListeners[ point ] == caller && System.Object.ReferenceEquals( eventListeners[ point ], callback)){
                // Debug.Log("This event is already being listened for.");
                return;
            }
#else
            if (goListeners[point] == caller && System.Object.Equals(eventListeners[point], callback))
            {
                // Debug.Log("This event is already being listened for.");
                return;
            }
#endif
        }
        Debug.LogError("You ran out of areas to add listeners, consider increasing LISTENERS_MAX, ex: LeanTween.LISTENERS_MAX = " + (LISTENERS_MAX * 2));
    }

    public static bool removeListener(int eventId, System.Action<LTEvent> callback)
    {
        return removeListener(tweenEmpty, eventId, callback);
    }

    /// <summary>
    /// Remove an event listener you have added
    /// <summary>
    /// <param name="caller">the gameObject the listener is attached to</param>
    /// <param name="eventId">a unique int that describes the event (best to use an enum)</param>
    /// <param name="callback">the method that was specified to call when the event has been dispatched</param>
    public static bool removeListener(GameObject caller, int eventId, System.Action<LTEvent> callback)
    {
        for (i = 0; i < eventsMaxSearch; i++)
        {
            int point = eventId * INIT_LISTENERS_MAX + i;
#if UNITY_FLASH
            if(goListeners[ point ] == caller && System.Object.ReferenceEquals( eventListeners[ point ], callback) ){
#else
            if (goListeners[point] == caller && System.Object.Equals(eventListeners[point], callback))
            {
#endif
                eventListeners[point] = null;
                goListeners[point] = null;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Tell the added listeners that you are dispatching the event
    /// <summary>
    /// <param name="eventId">a unique int that describes the event (best to use an enum)</param>
    public static void dispatchEvent(int eventId)
    {
        dispatchEvent(eventId, null);
    }

    /// <summary>
    /// Tell the added listeners that you are dispatching the event
    /// <summary>
    /// <param name="eventId">a unique int that describes the event (best to use an enum)</param>
    /// <param name="data">Pass data to the listener, access it from the listener with *.data on the LTEvent object</param>
    public static void dispatchEvent(int eventId, object data)
    {
        for (int k = 0; k < eventsMaxSearch; k++)
        {
            int point = eventId * INIT_LISTENERS_MAX + k;
            if (eventListeners[point] != null)
            {
                if (goListeners[point])
                {
                    eventListeners[point](new LTEvent(eventId, data));
                }
                else
                {
                    eventListeners[point] = null;
                }
            }
        }
    }


} // End LeanTween class

public class LTUtility
{

    public static Vector3[] reverse(Vector3[] arr)
    {
        int length = arr.Length;
        int left = 0;
        int right = length - 1;

        for (; left < right; left += 1, right -= 1)
        {
            Vector3 temporary = arr[left];
            arr[left] = arr[right];
            arr[right] = temporary;
        }
        return arr;
    }
}

public class LTBezier
{
    public float length;

    private Vector3 a;
    private Vector3 aa;
    private Vector3 bb;
    private Vector3 cc;
    private float len;
    private float[] arcLengths;

    public LTBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float precision)
    {
        this.a = a;
        aa = (-a + 3 * (b - c) + d);
        bb = 3 * (a + c) - 6 * b;
        cc = 3 * (b - a);

        this.len = 1.0f / precision;
        arcLengths = new float[(int)this.len + (int)1];
        arcLengths[0] = 0;

        Vector3 ov = a;
        Vector3 v;
        float clen = 0.0f;
        for (int i = 1; i <= this.len; i++)
        {
            v = bezierPoint(i * precision);
            clen += (ov - v).magnitude;
            this.arcLengths[i] = clen;
            ov = v;
        }
        this.length = clen;
    }

    private float map(float u)
    {
        float targetLength = u * this.arcLengths[(int)this.len];
        int low = 0;
        int high = (int)this.len;
        int index = 0;
        while (low < high)
        {
            index = low + ((int)((high - low) / 2.0f) | 0);
            if (this.arcLengths[index] < targetLength)
            {
                low = index + 1;
            }
            else
            {
                high = index;
            }
        }
        if (this.arcLengths[index] > targetLength)
            index--;
        if (index < 0)
            index = 0;

        return (index + (targetLength - arcLengths[index]) / (arcLengths[index + 1] - arcLengths[index])) / this.len;
    }

    private Vector3 bezierPoint(float t)
    {
        return ((aa * t + (bb)) * t + cc) * t + a;
    }

    public Vector3 point(float t)
    {
        return bezierPoint(map(t));
    }
}

public class LTBezierPath
{
    public Vector3[] pts;
    public float length;
    public bool orientToPath;
    public bool orientToPath2d;

    private LTBezier[] beziers;
    private float[] lengthRatio;
    private int currentBezier = 0, previousBezier = 0;

    public LTBezierPath() { }
    public LTBezierPath(Vector3[] pts_)
    {
        setPoints(pts_);
    }

    public void setPoints(Vector3[] pts_)
    {
        if (pts_.Length < 4)
            LeanTween.logError("LeanTween - When passing values for a vector path, you must pass four or more values!");
        if (pts_.Length % 4 != 0)
            LeanTween.logError("LeanTween - When passing values for a vector path, they must be in sets of four: controlPoint1, controlPoint2, endPoint2, controlPoint2, controlPoint2...");

        pts = pts_;

        int k = 0;
        beziers = new LTBezier[pts.Length / 4];
        lengthRatio = new float[beziers.Length];
        int i;
        length = 0;
        for (i = 0; i < pts.Length; i += 4)
        {
            beziers[k] = new LTBezier(pts[i + 0], pts[i + 2], pts[i + 1], pts[i + 3], 0.05f);
            length += beziers[k].length;
            k++;
        }
        // Debug.Log("beziers.Length:"+beziers.Length + " beziers:"+beziers);
        for (i = 0; i < beziers.Length; i++)
        {
            lengthRatio[i] = beziers[i].length / length;
        }
    }

    public float distance
    {
        get
        {
            return length;
        }
    }

    /// <summary>
    /// Retrieve a point along a path
    /// <summary>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    /// <returns>Vector3 position of the point along the path</returns>
    public Vector3 point(float ratio)
    {
        float added = 0.0f;
        for (int i = 0; i < lengthRatio.Length; i++)
        {
            added += lengthRatio[i];
            if (added >= ratio)
                return beziers[i].point((ratio - (added - lengthRatio[i])) / lengthRatio[i]);
        }
        return beziers[lengthRatio.Length - 1].point(1.0f);
    }

    public void place2d(Transform transform, float ratio)
    {
        transform.position = point(ratio);
        ratio += 0.001f;
        if (ratio <= 1.0f)
        {
            Vector3 v3Dir = point(ratio) - transform.position;
            float angle = Mathf.Atan2(v3Dir.y, v3Dir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }

    public void placeLocal2d(Transform transform, float ratio)
    {
        transform.localPosition = point(ratio);
        ratio += 0.001f;
        if (ratio <= 1.0f)
        {
            Vector3 v3Dir = transform.parent.TransformPoint(point(ratio)) - transform.localPosition;
            float angle = Mathf.Atan2(v3Dir.y, v3Dir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }

    /// <summary>
    /// Place an object along a certain point on the path (facing the direction perpendicular to the path)
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    public void place(Transform transform, float ratio)
    {
        place(transform, ratio, Vector3.up);

    }

    /// <summary>
    /// Place an object along a certain point on the path, with it facing a certain direction perpendicular to the path
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    /// <param name="rotation">the direction in which to place the transform ex: Vector3.up</param>
    public void place(Transform transform, float ratio, Vector3 worldUp)
    {
        transform.position = point(ratio);
        ratio += 0.001f;
        if (ratio <= 1.0f)
            transform.LookAt(point(ratio), worldUp);

    }

    /// <summary>
    /// Place an object along a certain point on the path (facing the direction perpendicular to the path) - Local Space, not world-space
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    public void placeLocal(Transform transform, float ratio)
    {
        placeLocal(transform, ratio, Vector3.up);
    }

    /// <summary>
    /// Place an object along a certain point on the path, with it facing a certain direction perpendicular to the path - Local Space, not world-space
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    /// <param name="rotation">the direction in which to place the transform ex: Vector3.up</param>
    public void placeLocal(Transform transform, float ratio, Vector3 worldUp)
    {
        ratio = getRationInOneRange(ratio);
        transform.localPosition = point(ratio);
        ratio = getRationInOneRange(ratio + 0.001f);
        if (ratio <= 1.0f)
            transform.LookAt(transform.parent.TransformPoint(point(ratio)), worldUp);
    }

    public float getRationInOneRange(float ratio)
    {
        if (ratio >= 0.0f && ratio <= 1.0f)
        {
            return ratio;
        }
        else if (ratio < 0.0f)
        {
            return Mathf.Ceil(ratio) - ratio;	//if -1.4 => it returns 0.4
        }
        else
        {
            return ratio - Mathf.Floor(ratio);	//if 1.4 => it return 0.4
        }
    }

    public void gizmoDraw(float t = -1.0f)
    {
        Vector3 prevPt = point(0);

        for (int i = 1; i <= 120; i++)
        {
            float pm = (float)i / 120f;
            Vector3 currPt2 = point(pm);
            //Gizmos.color = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1);
            Gizmos.color = (previousBezier == currentBezier) ? Color.magenta : Color.grey;
            Gizmos.DrawLine(currPt2, prevPt);
            prevPt = currPt2;
            previousBezier = currentBezier;
        }
    }
}

[System.Serializable]
public class LTSpline
{
    public static int DISTANCE_COUNT = 3; // increase for a more accurate constant speed
    public static int SUBLINE_COUNT = 20; // increase for a more accurate smoothing of the curves into lines

    public float distance = 0f;

    public bool constantSpeed = true;

    public Vector3[] pts;
    [System.NonSerialized]
    public Vector3[] ptsAdj;
    public int ptsAdjLength;
    public bool orientToPath;
    public bool orientToPath2d;
    private int numSections;
    private int currPt;

    public LTSpline(Vector3[] pts)
    {
        init(pts, true);
    }

    public LTSpline(Vector3[] pts, bool constantSpeed)
    {
        this.constantSpeed = constantSpeed;
        init(pts, constantSpeed);
    }

    private void init(Vector3[] pts, bool constantSpeed)
    {
        if (pts.Length < 4)
        {
            LeanTween.logError("LeanTween - When passing values for a spline path, you must pass four or more values!");
            return;
        }

        this.pts = new Vector3[pts.Length];
        System.Array.Copy(pts, this.pts, pts.Length);

        numSections = pts.Length - 3;

        float minSegment = float.PositiveInfinity;
        Vector3 earlierPoint = this.pts[1];
        float totalDistance = 0f;
        for (int i = 1; i < this.pts.Length - 1; i++)
        {
            // float pointDistance = (this.pts[i]-earlierPoint).sqrMagnitude;
            float pointDistance = Vector3.Distance(this.pts[i], earlierPoint);
            //Debug.Log("pointDist:"+pointDistance);
            if (pointDistance < minSegment)
            {
                minSegment = pointDistance;
            }

            totalDistance += pointDistance;
        }

        if (constantSpeed)
        {
            minSegment = totalDistance / (numSections * SUBLINE_COUNT);
            //Debug.Log("minSegment:"+minSegment+" numSections:"+numSections);

            float minPrecision = minSegment / SUBLINE_COUNT; // number of subdivisions in each segment
            int precision = (int)Mathf.Ceil(totalDistance / minPrecision) * DISTANCE_COUNT;
            // Debug.Log("precision:"+precision);
            if (precision <= 1) // precision has to be greater than one
                precision = 2;

            ptsAdj = new Vector3[precision];
            earlierPoint = interp(0f);
            int num = 1;
            ptsAdj[0] = earlierPoint;
            distance = 0f;
            for (int i = 0; i < precision + 1; i++)
            {
                float fract = ((float)(i)) / precision;
                // Debug.Log("fract:"+fract);
                Vector3 point = interp(fract);
                float dist = Vector3.Distance(point, earlierPoint);

                // float dist = (point-earlierPoint).sqrMagnitude;
                if (dist >= minPrecision || fract >= 1.0f)
                {
                    ptsAdj[num] = point;
                    distance += dist; // only add it to the total distance once we know we are adding it as an adjusted point

                    earlierPoint = point;
                    // Debug.Log("fract:"+fract+" point:"+point);
                    num++;
                }
            }
            // make sure there is a point at the very end
            // Debug.Log("fract 1f endPoint:"+endPoint);

            ptsAdjLength = num;
        }
        // Debug.Log("map 1f:"+map(1f)+" end:"+ptsAdj[ ptsAdjLength-1 ]);

        // Debug.Log("ptsAdjLength:"+ptsAdjLength+" minPrecision:"+minPrecision+" precision:"+precision);
    }

    public Vector3 map(float u)
    {
        if (u >= 1f)
            return pts[pts.Length - 2];
        float t = u * (ptsAdjLength - 1);
        int first = (int)Mathf.Floor(t);
        int next = (int)Mathf.Ceil(t);

        if (first < 0)
            first = 0;

        Vector3 val = ptsAdj[first];


        Vector3 nextVal = ptsAdj[next];
        float diff = t - first;

        // Debug.Log("u:"+u+" val:"+val +" nextVal:"+nextVal+" diff:"+diff+" first:"+first+" next:"+next);

        val = val + (nextVal - val) * diff;

        return val;
    }

    public Vector3 interp(float t)
    {
        currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
        float u = t * (float)numSections - (float)currPt;

        //Debug.Log("currPt:"+currPt+" numSections:"+numSections+" pts.Length :"+pts.Length );
        Vector3 a = pts[currPt];
        Vector3 b = pts[currPt + 1];
        Vector3 c = pts[currPt + 2];
        Vector3 d = pts[currPt + 3];

        Vector3 val = (.5f * (
        (-a + 3f * b - 3f * c + d) * (u * u * u)
        + (2f * a - 5f * b + 4f * c - d) * (u * u)
        + (-a + c) * u
        + 2f * b));
        // Debug.Log("currPt:"+currPt+" t:"+t+" val.x"+val.x+" y:"+val.y+" z:"+val.z);

        return val;
    }

    /// <summary>
    /// Retrieve a point along a path
    /// <summary>
    /// <param name="point">given a current location it makes the best approximiation of where it is along the path ratio-wise (0-1)</param>
    /// <returns>float of ratio along the path</returns>
    public float ratioAtPoint(Vector3 pt)
    {
        float closestDist = float.MaxValue;
        int closestI = 0;
        for (int i = 0; i < ptsAdjLength; i++)
        {
            float dist = Vector3.Distance(pt, ptsAdj[i]);
            // Debug.Log("i:"+i+" dist:"+dist);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestI = i;
            }
        }
        // Debug.Log("closestI:"+closestI+" ptsAdjLength:"+ptsAdjLength);
        return (float)closestI / (float)(ptsAdjLength - 1);
    }

    /// <summary>
    /// Retrieve a point along a path
    /// <summary>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    /// <returns>Vector3 position of the point along the path</returns>
    public Vector3 point(float ratio)
    {
        float t = ratio > 1f ? 1f : ratio;
        return constantSpeed ? map(t) : interp(t);
    }

    public void place2d(Transform transform, float ratio)
    {
        transform.position = point(ratio);
        ratio += 0.001f;
        if (ratio <= 1.0f)
        {
            Vector3 v3Dir = point(ratio) - transform.position;
            float angle = Mathf.Atan2(v3Dir.y, v3Dir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }

    public void placeLocal2d(Transform transform, float ratio)
    {
        transform.localPosition = point(ratio);
        ratio += 0.001f;
        if (ratio <= 1.0f)
        {
            Vector3 v3Dir = transform.parent.TransformPoint(point(ratio)) - transform.localPosition;
            float angle = Mathf.Atan2(v3Dir.y, v3Dir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }


    /// <summary>
    /// Place an object along a certain point on the path (facing the direction perpendicular to the path)
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    public void place(Transform transform, float ratio)
    {
        place(transform, ratio, Vector3.up);
    }

    /// <summary>
    /// Place an object along a certain point on the path, with it facing a certain direction perpendicular to the path
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    /// <param name="rotation">the direction in which to place the transform ex: Vector3.up</param>
    public void place(Transform transform, float ratio, Vector3 worldUp)
    {
        // ratio = Mathf.Repeat(ratio, 1.0f); // make sure ratio is always between 0-1
        transform.position = point(ratio);
        ratio += 0.001f;
        if (ratio <= 1.0f)
            transform.LookAt(point(ratio), worldUp);

    }

    /// <summary>
    /// Place an object along a certain point on the path (facing the direction perpendicular to the path) - Local Space, not world-space
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    public void placeLocal(Transform transform, float ratio)
    {
        placeLocal(transform, ratio, Vector3.up);
    }

    /// <summary>
    /// Place an object along a certain point on the path, with it facing a certain direction perpendicular to the path - Local Space, not world-space
    /// <summary>
    /// <param name="transform">the transform of the object you wish to place along the path</param>
    /// <param name="ratio">ratio of the point along the path you wish to receive (0-1)</param>
    /// <param name="rotation">the direction in which to place the transform ex: Vector3.up</param>
    public void placeLocal(Transform transform, float ratio, Vector3 worldUp)
    {
        transform.localPosition = point(ratio);
        ratio += 0.001f;
        if (ratio <= 1.0f)
            transform.LookAt(transform.parent.TransformPoint(point(ratio)), worldUp);
    }

    public void gizmoDraw(float t = -1.0f)
    {
        if (ptsAdj == null || ptsAdj.Length <= 0)
            return;

        Vector3 prevPt = ptsAdj[0];

        for (int i = 0; i < ptsAdjLength; i++)
        {
            Vector3 currPt2 = ptsAdj[i];
            // Debug.Log("currPt2:"+currPt2);
            //Gizmos.color = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1);
            Gizmos.DrawLine(prevPt, currPt2);
            prevPt = currPt2;
        }
    }

    public void drawGizmo(Color color)
    {
        if (this.ptsAdjLength >= 4)
        {

            Vector3 prevPt = this.ptsAdj[0];

            Color colorBefore = Gizmos.color;
            Gizmos.color = color;
            for (int i = 0; i < this.ptsAdjLength; i++)
            {
                Vector3 currPt2 = this.ptsAdj[i];
                // Debug.Log("currPt2:"+currPt2);

                Gizmos.DrawLine(prevPt, currPt2);
                prevPt = currPt2;
            }
            Gizmos.color = colorBefore;
        }
    }

    public static void drawGizmo(Transform[] arr, Color color)
    {
        if (arr.Length >= 4)
        {
            Vector3[] vec3s = new Vector3[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                vec3s[i] = arr[i].position;
            }
            LTSpline spline = new LTSpline(vec3s);
            Vector3 prevPt = spline.ptsAdj[0];

            Color colorBefore = Gizmos.color;
            Gizmos.color = color;
            for (int i = 0; i < spline.ptsAdjLength; i++)
            {
                Vector3 currPt2 = spline.ptsAdj[i];
                // Debug.Log("currPt2:"+currPt2);

                Gizmos.DrawLine(prevPt, currPt2);
                prevPt = currPt2;
            }
            Gizmos.color = colorBefore;
        }
    }


    public static void drawLine(Transform[] arr, float width, Color color)
    {
        if (arr.Length >= 4)
        {

        }
    }


    public void drawLinesGLLines(Material outlineMaterial, Color color, float width)
    {
        GL.PushMatrix();
        outlineMaterial.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.LINES);
        GL.Color(color);

        if (constantSpeed)
        {
            if (this.ptsAdjLength >= 4)
            {

                Vector3 prevPt = this.ptsAdj[0];

                for (int i = 0; i < this.ptsAdjLength; i++)
                {
                    Vector3 currPt2 = this.ptsAdj[i];
                    GL.Vertex(prevPt);
                    GL.Vertex(currPt2);

                    prevPt = currPt2;
                }
            }

        }
        else
        {
            if (this.pts.Length >= 4)
            {

                Vector3 prevPt = this.pts[0];

                float split = 1f / ((float)this.pts.Length * 10f);

                float iter = 0f;
                while (iter < 1f)
                {
                    float at = iter / 1f;
                    Vector3 currPt2 = interp(at);
                    // Debug.Log("currPt2:"+currPt2);

                    GL.Vertex(prevPt);
                    GL.Vertex(currPt2);

                    prevPt = currPt2;

                    iter += split;
                }
            }
        }


        GL.End();
        GL.PopMatrix();

    }

    public Vector3[] generateVectors()
    {
        if (this.pts.Length >= 4)
        {
            List<Vector3> meshPoints = new List<Vector3>();
            Vector3 prevPt = this.pts[0];
            meshPoints.Add(prevPt);

            float split = 1f / ((float)this.pts.Length * 10f);

            float iter = 0f;
            while (iter < 1f)
            {
                float at = iter / 1f;
                Vector3 currPt2 = interp(at);
                //                Debug.Log("currPt2:"+currPt2);

                //                GL.Vertex(prevPt);
                //                GL.Vertex(currPt2);
                meshPoints.Add(currPt2);

                //                prevPt = currPt2;

                iter += split;
            }

            meshPoints.ToArray();
        }
        return null;
    }
}

/// <summary>
/// Animate GUI Elements by creating this object and passing the *.rect variable to the GUI method<br><br>
/// <summary>
/// <param name="x">X location</param>
/// <param name="y">Y location</param>
/// <param name="width">Width</param>
/// <param name="height">Height</param>
/// <param name="alpha">(Optional) initial alpha amount (0-1)</param>
/// <param name="rotation">(Optional) initial rotation in degrees (0-360)</param>

[System.Serializable]
public class LTRect : System.Object
{
    public Rect _rect;
    public float alpha = 1f;
    public float rotation;
    public Vector2 pivot;
    public Vector2 margin;
    public Rect relativeRect = new Rect(0f, 0f, float.PositiveInfinity, float.PositiveInfinity);

    public bool rotateEnabled;
    [HideInInspector]
    public bool rotateFinished;
    public bool alphaEnabled;
    public string labelStr;
    public LTGUI.Element_Type type;
    public GUIStyle style;
    public bool useColor = false;
    public Color color = Color.white;
    public bool fontScaleToFit;
    public bool useSimpleScale;
    public bool sizeByHeight;

    public Texture texture;

    private int _id = -1;
    [HideInInspector]
    public int counter;

    public static bool colorTouched;

    public LTRect()
    {
        reset();
        this.rotateEnabled = this.alphaEnabled = true;
        _rect = new Rect(0f, 0f, 1f, 1f);
    }

    public LTRect(Rect rect)
    {
        _rect = rect;
        reset();
    }

    public LTRect(float x, float y, float width, float height)
    {
        _rect = new Rect(x, y, width, height);
        this.alpha = 1.0f;
        this.rotation = 0.0f;
        this.rotateEnabled = this.alphaEnabled = false;
    }

    public LTRect(float x, float y, float width, float height, float alpha)
    {
        _rect = new Rect(x, y, width, height);
        this.alpha = alpha;
        this.rotation = 0.0f;
        this.rotateEnabled = this.alphaEnabled = false;
    }

    public LTRect(float x, float y, float width, float height, float alpha, float rotation)
    {
        _rect = new Rect(x, y, width, height);
        this.alpha = alpha;
        this.rotation = rotation;
        this.rotateEnabled = this.alphaEnabled = false;
        if (rotation != 0.0f)
        {
            this.rotateEnabled = true;
            resetForRotation();
        }
    }

    public bool hasInitiliazed
    {
        get
        {
            return _id != -1;
        }
    }

    public int id
    {
        get
        {
            int toId = _id | counter << 16;


            return toId;
        }
    }

    public void setId(int id, int counter)
    {
        this._id = id;
        this.counter = counter;
    }

    public void reset()
    {
        this.alpha = 1.0f;
        this.rotation = 0.0f;
        this.rotateEnabled = this.alphaEnabled = false;
        this.margin = Vector2.zero;
        this.sizeByHeight = false;
        this.useColor = false;
    }

    public void resetForRotation()
    {
        Vector3 scale = new Vector3(GUI.matrix[0, 0], GUI.matrix[1, 1], GUI.matrix[2, 2]);
        if (pivot == Vector2.zero)
        {
            pivot = new Vector2((_rect.x + ((_rect.width) * 0.5f)) * scale.x + GUI.matrix[0, 3], (_rect.y + ((_rect.height) * 0.5f)) * scale.y + GUI.matrix[1, 3]);
        }
    }

    public float x
    {
        get { return _rect.x; }
        set { _rect.x = value; }
    }

    public float y
    {
        get { return _rect.y; }
        set { _rect.y = value; }
    }

    public float width
    {
        get { return _rect.width; }
        set { _rect.width = value; }
    }

    public float height
    {
        get { return _rect.height; }
        set { _rect.height = value; }
    }

    public Rect rect
    {

        get
        {
            if (colorTouched)
            {
                colorTouched = false;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1.0f);
            }
            if (rotateEnabled)
            {
                if (rotateFinished)
                {
                    rotateFinished = false;
                    rotateEnabled = false;
                    //this.rotation = 0.0f;
                    pivot = Vector2.zero;
                }
                else
                {
                    GUIUtility.RotateAroundPivot(rotation, pivot);
                }
            }
            if (alphaEnabled)
            {
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
                colorTouched = true;
            }
            if (fontScaleToFit)
            {
                if (this.useSimpleScale)
                {
                    style.fontSize = (int)(_rect.height * this.relativeRect.height);
                }
                else
                {
                    style.fontSize = (int)_rect.height;
                }
            }
            return _rect;
        }

        set
        {
            _rect = value;
        }
    }

    public LTRect setStyle(GUIStyle style)
    {
        this.style = style;
        return this;
    }

    public LTRect setFontScaleToFit(bool fontScaleToFit)
    {
        this.fontScaleToFit = fontScaleToFit;
        return this;
    }

    public LTRect setColor(Color color)
    {
        this.color = color;
        this.useColor = true;
        return this;
    }

    public LTRect setAlpha(float alpha)
    {
        this.alpha = alpha;
        return this;
    }

    public LTRect setLabel(String str)
    {
        this.labelStr = str;
        return this;
    }

    public LTRect setUseSimpleScale(bool useSimpleScale, Rect relativeRect)
    {
        this.useSimpleScale = useSimpleScale;
        this.relativeRect = relativeRect;
        return this;
    }

    public LTRect setUseSimpleScale(bool useSimpleScale)
    {
        this.useSimpleScale = useSimpleScale;
        this.relativeRect = new Rect(0f, 0f, Screen.width, Screen.height);
        return this;
    }

    public LTRect setSizeByHeight(bool sizeByHeight)
    {
        this.sizeByHeight = sizeByHeight;
        return this;
    }

    public override string ToString()
    {
        return "x:" + _rect.x + " y:" + _rect.y + " width:" + _rect.width + " height:" + _rect.height;
    }
}

/// <summary>
/// Object that describes the event to an event listener
/// <summary>
/// <param name="data">Data that has been passed from the dispatchEvent method</param>
public class LTEvent
{
    public int id;
    public object data;

    public LTEvent(int id, object data)
    {
        this.id = id;
        this.data = data;
    }
}

public class LTGUI
{
    public static int RECT_LEVELS = 5;
    public static int RECTS_PER_LEVEL = 10;
    public static int BUTTONS_MAX = 24;

    private static LTRect[] levels;
    private static int[] levelDepths;
    private static Rect[] buttons;
    private static int[] buttonLevels;
    private static int[] buttonLastFrame;
    private static LTRect r;
    private static Color color = Color.white;
    private static bool isGUIEnabled = false;
    private static int global_counter = 0;

    public enum Element_Type
    {
        Texture,
        Label
    }

    public static void init()
    {
        if (levels == null)
        {
            levels = new LTRect[RECT_LEVELS * RECTS_PER_LEVEL];
            levelDepths = new int[RECT_LEVELS];
        }
    }

    public static void initRectCheck()
    {
        if (buttons == null)
        {
            buttons = new Rect[BUTTONS_MAX];
            buttonLevels = new int[BUTTONS_MAX];
            buttonLastFrame = new int[BUTTONS_MAX];
            for (int i = 0; i < buttonLevels.Length; i++)
            {
                buttonLevels[i] = -1;
            }
        }
    }

    public static void reset()
    {
        if (isGUIEnabled)
        {
            isGUIEnabled = false;
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i] = null;
            }

            for (int i = 0; i < levelDepths.Length; i++)
            {
                levelDepths[i] = 0;
            }
        }
    }

    public static void update(int updateLevel)
    {
        if (isGUIEnabled)
        {
            init();
            if (levelDepths[updateLevel] > 0)
            {
                color = GUI.color;
                int baseI = updateLevel * RECTS_PER_LEVEL;
                int maxLoop = baseI + levelDepths[updateLevel];// RECTS_PER_LEVEL;//;

                for (int i = baseI; i < maxLoop; i++)
                {
                    r = levels[i];
                    // Debug.Log("r:"+r+" i:"+i);
                    if (r != null /*&& checkOnScreen(r.rect)*/)
                    {
                        //Debug.Log("label:"+r.labelStr+" textColor:"+r.style.normal.textColor);
                        if (r.useColor)
                            GUI.color = r.color;
                        if (r.type == Element_Type.Label)
                        {
                            if (r.style != null)
                                GUI.skin.label = r.style;
                            if (r.useSimpleScale)
                            {
                                GUI.Label(new Rect((r.rect.x + r.margin.x + r.relativeRect.x) * r.relativeRect.width, (r.rect.y + r.margin.y + r.relativeRect.y) * r.relativeRect.height, r.rect.width * r.relativeRect.width, r.rect.height * r.relativeRect.height), r.labelStr);
                            }
                            else
                            {
                                GUI.Label(new Rect(r.rect.x + r.margin.x, r.rect.y + r.margin.y, r.rect.width, r.rect.height), r.labelStr);
                            }
                        }
                        else if (r.type == Element_Type.Texture && r.texture != null)
                        {
                            Vector2 size = r.useSimpleScale ? new Vector2(0f, r.rect.height * r.relativeRect.height) : new Vector2(r.rect.width, r.rect.height);
                            if (r.sizeByHeight)
                            {
                                size.x = (float)r.texture.width / (float)r.texture.height * size.y;
                            }
                            if (r.useSimpleScale)
                            {
                                GUI.DrawTexture(new Rect((r.rect.x + r.margin.x + r.relativeRect.x) * r.relativeRect.width, (r.rect.y + r.margin.y + r.relativeRect.y) * r.relativeRect.height, size.x, size.y), r.texture);
                            }
                            else
                            {
                                GUI.DrawTexture(new Rect(r.rect.x + r.margin.x, r.rect.y + r.margin.y, size.x, size.y), r.texture);
                            }
                        }
                    }
                }
                GUI.color = color;
            }
        }
    }

    public static bool checkOnScreen(Rect rect)
    {
        bool offLeft = rect.x + rect.width < 0f;
        bool offRight = rect.x > Screen.width;
        bool offBottom = rect.y > Screen.height;
        bool offTop = rect.y + rect.height < 0f;

        return !(offLeft || offRight || offBottom || offTop);
    }

    public static void destroy(int id)
    {
        int backId = id & 0xFFFF;
        int backCounter = id >> 16;
        if (id >= 0 && levels[backId] != null && levels[backId].hasInitiliazed && levels[backId].counter == backCounter)
            levels[backId] = null;
    }

    public static void destroyAll(int depth)
    { // clears all gui elements on depth
        int maxLoop = depth * RECTS_PER_LEVEL + RECTS_PER_LEVEL;
        for (int i = depth * RECTS_PER_LEVEL; levels != null && i < maxLoop; i++)
        {
            levels[i] = null;
        }
    }

    public static LTRect label(Rect rect, string label, int depth)
    {
        return LTGUI.label(new LTRect(rect), label, depth);
    }

    public static LTRect label(LTRect rect, string label, int depth)
    {
        rect.type = Element_Type.Label;
        rect.labelStr = label;
        return element(rect, depth);
    }

    public static LTRect texture(Rect rect, Texture texture, int depth)
    {
        return LTGUI.texture(new LTRect(rect), texture, depth);
    }

    public static LTRect texture(LTRect rect, Texture texture, int depth)
    {
        rect.type = Element_Type.Texture;
        rect.texture = texture;
        return element(rect, depth);
    }

    public static LTRect element(LTRect rect, int depth)
    {
        isGUIEnabled = true;
        init();
        int maxLoop = depth * RECTS_PER_LEVEL + RECTS_PER_LEVEL;
        int k = 0;
        if (rect != null)
        {
            destroy(rect.id);
        }
        if (rect.type == LTGUI.Element_Type.Label && rect.style != null)
        {
            if (rect.style.normal.textColor.a <= 0f)
            {
                Debug.LogWarning("Your GUI normal color has an alpha of zero, and will not be rendered.");
            }
        }
        if (rect.relativeRect.width == float.PositiveInfinity)
        {
            rect.relativeRect = new Rect(0f, 0f, Screen.width, Screen.height);
        }
        for (int i = depth * RECTS_PER_LEVEL; i < maxLoop; i++)
        {
            r = levels[i];
            if (r == null)
            {
                r = rect;
                r.rotateEnabled = true;
                r.alphaEnabled = true;
                r.setId(i, global_counter);
                levels[i] = r;
                // Debug.Log("k:"+k+ " maxDepth:"+levelDepths[depth]);
                if (k >= levelDepths[depth])
                {
                    levelDepths[depth] = k + 1;
                }
                global_counter++;
                return r;
            }
            k++;
        }

        Debug.LogError("You ran out of GUI Element spaces");

        return null;
    }

    public static bool hasNoOverlap(Rect rect, int depth)
    {
        initRectCheck();
        bool hasNoOverlap = true;
        bool wasAddedToList = false;
        for (int i = 0; i < buttonLevels.Length; i++)
        {
            // Debug.Log("buttonLastFrame["+i+"]:"+buttonLastFrame[i]);
            //Debug.Log("buttonLevels["+i+"]:"+buttonLevels[i]);
            if (buttonLevels[i] >= 0)
            {
                //Debug.Log("buttonLastFrame["+i+"]:"+buttonLastFrame[i]+" Time.frameCount:"+Time.frameCount);
                if (buttonLastFrame[i] + 1 < Time.frameCount)
                { // It has to have been visible within the current, or
                    buttonLevels[i] = -1;
                    // Debug.Log("resetting i:"+i);
                }
                else
                {
                    //if(buttonLevels[i]>=0)
                    //	 Debug.Log("buttonLevels["+i+"]:"+buttonLevels[i]);
                    if (buttonLevels[i] > depth)
                    {
                        if (pressedWithinRect(buttons[i]))
                        {
                            hasNoOverlap = false; // there is an overlapping button that is higher
                        }
                    }
                }
            }

            if (wasAddedToList == false && buttonLevels[i] < 0)
            {
                wasAddedToList = true;
                buttonLevels[i] = depth;
                buttons[i] = rect;
                buttonLastFrame[i] = Time.frameCount;
            }
        }

        return hasNoOverlap;
    }

    public static bool pressedWithinRect(Rect rect)
    {
        Vector2 vec2 = firstTouch();
        if (vec2.x < 0f)
            return false;
        float vecY = Screen.height - vec2.y;
        return (vec2.x > rect.x && vec2.x < rect.x + rect.width && vecY > rect.y && vecY < rect.y + rect.height);
    }

    public static bool checkWithinRect(Vector2 vec2, Rect rect)
    {
        vec2.y = Screen.height - vec2.y;
        return (vec2.x > rect.x && vec2.x < rect.x + rect.width && vec2.y > rect.y && vec2.y < rect.y + rect.height);
    }

    public static Vector2 firstTouch()
    {
        if (Input.touchCount > 0)
        {
            return Input.touches[0].position;
        }
        else if (Input.GetMouseButton(0))
        {
            return Input.mousePosition;
        }

        return new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);
    }
}