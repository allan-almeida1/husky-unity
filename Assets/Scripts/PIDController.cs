using UnityEngine;

/// <summary>
/// A simple PID controller implementation.
/// </summary>
public class PIDController {
    private float kp, ki, kd;
    private float integral = 0.0f;
    private float previousError = 0.0f;

    public PIDController(float kp, float ki, float kd) {
        this.kp = kp;
        this.ki = ki;
        this.kd = kd;
    }

    public float Calculate(float targetVelocity, float currentVelocity, float deltaTime) {
        float error = targetVelocity - currentVelocity;
        integral += error * deltaTime;
        float derivative = (error - previousError) / deltaTime;
        previousError = error;

        return kp * error + ki * integral + kd * derivative;
    }
}
