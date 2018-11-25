using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Trajectory : MonoBehaviour {

	// fields
	public float launchAngle, initialVelocity, pointTimeInterval, tickIntervalDistance, drawTimeout;
	public bool runInRealTime;

	// variables
	private float	ballMass = 0.04593f,
					ballProjectedSurfaceArea = 0.0014f,
					ballDragCoefficient = 0.2f,
					environmentGravity = 9.81f,
					environmentAirDensity = 1.225f;
	private List<Vector2> points = new List<Vector2>();

	private void Start() {

		//TODO: call this again on change in launchAngle or initialVelocity
		StartCoroutine(evaluateTrajectory());
	}

	// hyperbolic cosine, needed for y(t) calculation
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
		float k = (p*A*C)/2;
		float xAtPeak = Mathf.Sqrt(m/(g*k))*Mathf.Atan(v * Mathf.Sin(n) * Mathf.Sqrt(k/m*g));

		// chunks used in y(t) calculation
		float _b = (m/(2*k))*(Mathf.Log((((k*(Mathf.Pow(v,2)))/(m*g))*(Mathf.Pow(Mathf.Sin(n),2)))+1));
		float _c = Mathf.Atan(v*Mathf.Sin(n)*Mathf.Sqrt(k/(m*g)));
		float _d = Mathf.Sqrt((g*k)/m);

		// clear points dictionary
		points = new List<Vector2>();

		// set first point to (0, 0) because of course it is
		points.Add(Vector2.zero);

		// calculate positions (x,y) as a function of time (t)
		float t = 0;
		while(true) {

			// increment time
			t += pointTimeInterval;

			// x(t)
			float x = (Mathf.Log(((t*v*k*Mathf.Cos(n))/m)+1)*m)/k;

			// y(t)
			float y;
			if(x < xAtPeak) { //rising
				y = ((m/k)*Mathf.Log(Mathf.Cos((t*_d)-_c)))+_b;
			}
			else { //falling
				y = (-(m/k)*Mathf.Log(Cosh((t*_d)-_c)))+_b;
			}

			// add (x,y) to points list
			Vector2 newPoint = new Vector2(x, y);
			points.Add(newPoint);

			// break when landing
			// (right now it lands at y=0)
			if(newPoint.y < transform.position.y) {
				break;
			}

			// render, then calculate next point
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

		// iterate over every lien segment
		// (starting at 1 because 0->1 is the first segment)
		for(int i = 1; i < points.Count; i++) {

			// get the two points for this line segment
			Vector2 thisPoint = points[i];
			Vector2 prevPoint = points[i - 1];

			// draw line between every point
			Vector3 a = new Vector3(thisPoint.x, thisPoint.y, 0);
			Vector3 b = new Vector3(prevPoint.x, prevPoint.y, 0);
			Debug.DrawLine(b, a, Color.white, drawTimeout);

			// draw tick every tickIntervalDistance meters
			if(thisPoint.x > tickX) {
				tickX += tickIntervalDistance;
				Vector3 c = new Vector3(prevPoint.x, 0, 0);
				Debug.DrawLine(b, c, Color.gray, drawTimeout);
			}

			// draw one line to indicate peak height
			if(peaked == false && thisPoint.y < prevPoint.y) {
				peaked = true;
				Vector3 c = new Vector3(prevPoint.x, 0, 0);
				Debug.DrawLine(b, c, Color.white, drawTimeout);
			}
		}
	}
}
