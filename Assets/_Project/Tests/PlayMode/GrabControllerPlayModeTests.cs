using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Veil.Interaction;

namespace Veil.Tests.PlayMode
{
    public class GrabControllerPlayModeTests
    {
        [UnityTest]
        public IEnumerator HeldObject_ConvergesTowardHoldPoint()
        {
            var holderGo = new GameObject("Holder");
            var controller = holderGo.AddComponent<GrabController>();

            var objGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var rb = objGo.AddComponent<Rigidbody>();
            var grabbable = objGo.AddComponent<GrabbableObject>();
            objGo.transform.position = new Vector3(5f, 0f, 0f);

            var holdPoint = new GameObject("HoldPoint").transform;
            holdPoint.position = Vector3.zero;
            controller.SetHoldPoint(holdPoint);

            controller.Grab(grabbable);

            for (int i = 0; i < 60; i++)
            {
                controller.TickHold(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.Less(Vector3.Distance(objGo.transform.position, holdPoint.position), 0.5f);

            Object.Destroy(holderGo);
            Object.Destroy(objGo);
        }
    }
}
