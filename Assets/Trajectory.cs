using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Trajectory : MonoBehaviour {

	public float launchAngle, initialVelocity, pointTimeInterval, tickIntervalDistance, drawTimeout;
	public bool runInRealTime;

	private float	ballMass = 0.04593f,
					ballProjectedSurfaceArea = 0.0014f,
					ballDragCoefficient = 0.2f,
					environmentGravity = 9.81f,
					environmentAirDensity = 1.225f;

	private void Start() {

		//TODO: call this again on change in launchAngle or initialVelocity
		StartCoroutine(evaluateTrajectory());
	}

	private float Cosh(float n) {
		return (Mathf.Exp(n) + Mathf.Exp(-n)) / 2;
	}

	private IEnumerator evaluateTrajectory() {

		// map properties to letters for compressed math
		float	a = launchAngle,
				v = initialVelocity,
				m = ballMass,
				A = ballProjectedSurfaceArea,
				C = ballDragCoefficient,
				g = environmentGravity,
				p = environmentAirDensity;

		// lots of stupid math that I don't actually understand
		float n = (Mathf.PI/180)*a;
		float k = (1/2)*p*A*C;
		k = 0.000175175f;
		float _b = (m/(2*k))*(Mathf.Log((((k*(Mathf.Pow(v,2)))/(m*g))*(Mathf.Pow(Mathf.Sin(n),2)))+1));
		float xAtPeak = Mathf.Sqrt(m/(g*k))*Mathf.Atan(v * Mathf.Sin(n) * Mathf.Sqrt(k/m*g));

		// calculate y values
		float t = 0;
		List<Vector2> points = new List<Vector2>();
		while(true) {

			// more stupid math that I don't understand
			//float e = 2.7182818284590f; // Euler's number
			//float t = (m/(k*v*Mathf.Cos(n)))*((Mathf.Pow(e,(k*x/m)))-1);
			float x = (Mathf.Log(((t*v*k*Mathf.Cos(n))/m)+1)*m)/k;
			float _a = (t*Mathf.Sqrt((g*k)/m))-(Mathf.Atan(v*Mathf.Sin(n)*Mathf.Sqrt(k/(m*g))));

			// y(t) for rising
			if(x < xAtPeak) {
				points.Add(new Vector2(x, ((m/k) * Mathf.Log(Mathf.Cos(_a))) + _b));
			}

			// y(t) for falling
			else {
				points.Add(new Vector2(x, (-(m/k) * Mathf.Log(Cosh(_a))) + _b));
			}

			// break when landing
			// (right now it lands at y=0)
			if(points[points.Count - 1].y < transform.position.y) {
				break;
			}

			t += pointTimeInterval;
			renderTrajectory(points);
			if(runInRealTime) {
				yield return new WaitForSeconds(pointTimeInterval);
			}
		}

		// keep rendering, even after it's done calculating
		while(true) {
			renderTrajectory(points);
			yield return new WaitForSeconds(drawTimeout);
		}
	}

	private void renderTrajectory(List<Vector2> points) {

		int tickIntervalDistance = 10; // in meters
		int tickX = tickIntervalDistance;
		bool peaked = false;

		for(int i = 1; i < points.Count; i++) {

			// draw line between every point
			Vector3 a = new Vector3(
				points[i].x,
				points[i].y,
				0
			);
			Vector3 b = new Vector3(
				points[i - 1].x,
				points[i - 1].y,
				0
			);
			Debug.DrawLine(b, a, Color.white, drawTimeout);

			// draw tick every tickIntervalDistance meters
			if(points[i].x > tickX) {
				tickX += tickIntervalDistance;
				Vector3 c = new Vector3(
					points[i - 1].x,
					0,
					0
				);
				Debug.DrawLine(b, c, Color.gray, drawTimeout);
			}

			// draw one line to indicate peak height
			if(peaked == false && points[i].y < points[i - 1].y) {
				peaked = true;
				Vector3 c = new Vector3(
					points[i - 1].x,
					0,
					0
				);
				Debug.DrawLine(b, c, Color.white, drawTimeout);
			}
		}
	}
}
