using System;
using System.Collections;
using System.Linq;
using Code.Internal.UserInterface;
using Rewired;
using UnityEngine;

namespace Code.Internal.Input
{
    public class Calibration : MonoBehaviour
    {
        private Player _player;
        private bool _isCalibrating;

        public int _throttleAxisId, _yawAxisId, _pitchAxisId, _rollAxisId, _cameraButtonId, _modeButtonId, _restartButtonId;

        public static Calibration Instance;
        
        public bool IsCalibrating => _isCalibrating;

        private int axesCount;
        private float[] minValues;
        private float[] maxValues;
        private float[] zeroValues;

        private Joystick _findJoystick;
        public Joystick _joystick;

        public GameObject enableAfterFinish;

        public event Action<int> StepDone; 
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
            _player = ReInput.players.GetPlayer(0);
        }

        private void Update()
        {
            /*if (ReInput.controllers.joystickCount < 1)
            {
                enableAfterFinish.SetActive(true);
                gameObject.SetActive(false);
                return;
            }*/
            
            /*if (!_isCalibrating) 
                UISubtitle.Instance.SetTextInstant("Для калибровки контроллера нажмите любую клавишу");
                */

            UpdateJoystick();
            
            /*
            if ((UnityEngine.Input.anyKeyDown) && !_isCalibrating)
                StartCoroutine(CalibrateJoysticks());*/
        }
        
        private void UpdateJoystick()
        {
            if (_player.controllers.Joysticks.Count == 0) return;
            
            _findJoystick = null;
            
            foreach (var joystick in ReInput.controllers.Joysticks)
            {
                if (joystick.hardwareName.ToLower().Contains("flysky"))
                {
                    _findJoystick = joystick;
                    break;
                }
            }

            if (_findJoystick == null)
                _findJoystick = _player.controllers.Joysticks[0];

            if (_joystick != _findJoystick)
            {
                _joystick = _findJoystick;
                _player.controllers.Joysticks.Clear();
                _player.controllers.Joysticks.Add(_joystick);
            }
        }

        public void StartCalibration()
        {
            StartCoroutine(CalibrateJoysticks());
        }

        private IEnumerator CalibrateJoysticks()
        {
            _isCalibrating = true;

            UISubtitle.Instance.SetTextInstant("Калибровка начата. Вращайте джойстики по кругу.");
            yield return StartCoroutine(CalibrateExtremes());
            
            StepDone?.Invoke(0);

            UISubtitle.Instance.ClearText();
            yield return new WaitForSeconds(1f);

            UISubtitle.Instance.SetTextInstant("Переведите стики в центр.");
            yield return StartCoroutine(CalibrateZeros());

            ApplyCalibration();
            
            UISubtitle.Instance.ClearText();
            yield return new WaitForSeconds(1);
            UISubtitle.Instance.SetTextInstant("Левый стик вверх");
            //yield return StartCoroutine(WaitZeros());
            yield return StartCoroutine(CheckAxis(i => _throttleAxisId = i));
            StepDone?.Invoke(1);
            UISubtitle.Instance.ClearText();
            yield return new WaitForSeconds(1);
            UISubtitle.Instance.SetTextInstant("Левый стик вправо");
            //yield return StartCoroutine(WaitZeros());
            yield return StartCoroutine(CheckAxis(i => _yawAxisId = i));
            StepDone?.Invoke(2);
            UISubtitle.Instance.ClearText();
            yield return new WaitForSeconds(1);
            UISubtitle.Instance.SetTextInstant("Правый стик вверх");
            //yield return StartCoroutine(WaitZeros());
            yield return StartCoroutine(CheckAxis(i => _pitchAxisId = i));
            StepDone?.Invoke(3);
            UISubtitle.Instance.ClearText();
            yield return new WaitForSeconds(1);
            UISubtitle.Instance.SetTextInstant("Правый стик вправо");
            //yield return StartCoroutine(WaitZeros());
            yield return StartCoroutine(CheckAxis(i => _rollAxisId = i));
            StepDone?.Invoke(4);
            UISubtitle.Instance.ClearText();
            yield return new WaitForSeconds(1);
            
            /*UISubtitle.Instance.SetTextInstant("Кнопка переключения камеры");
            //yield return StartCoroutine(WaitZeros());
            yield return StartCoroutine(CheckButton (i => _cameraButtonId = i));
            yield return new WaitForSeconds(1);
            UISubtitle.Instance.SetTextInstant("Кнопка переключения режима управления");
            //yield return StartCoroutine(WaitZeros());
            yield return StartCoroutine(CheckButton (i => _modeButtonId = i));
            yield return new WaitForSeconds(1);
            UISubtitle.Instance.SetTextInstant("Кнопка рестарта дрона");
            //yield return StartCoroutine(WaitZeros());
            yield return StartCoroutine(CheckButton (i => _restartButtonId = i));*/
            
            BindAxes();

            UISubtitle.Instance.SetTextInstant("Калибровка завершена!");
            yield return new WaitForSeconds(2);
            UISubtitle.Instance.ClearText();
            _isCalibrating = false;
            enableAfterFinish?.SetActive(true);
            gameObject.SetActive(false);
        }

        private IEnumerator CalibrateExtremes()
        {
            var startMovingSticks = false;
            
            yield return new WaitUntil(() => { return _joystick.Axes.ToList().FirstOrDefault(a => a.valueDelta > 0.2f) != null; });

            UISubtitle.Instance.SetTextInstant("Продолжайте вращать джойстики по кругу.");

            axesCount = _joystick.Axes.Count;
            minValues = new float[axesCount];
            maxValues = new float[axesCount];
            zeroValues = new float[axesCount];
            
            for (var i = 0; i < axesCount; i++)
            {
                maxValues[i] = float.MinValue;
                minValues[i] = float.MaxValue;
                zeroValues[i] = 0f;
            }

            var currentTime = 0f;
            while (currentTime < 3f)
            {
                for (var i = 0; i < axesCount; i++)
                {
                    var currentAxisValue = _joystick.Axes[i].value;
                    if (currentAxisValue > maxValues[i]) maxValues[i] = currentAxisValue;
                    if (currentAxisValue < minValues[i]) minValues[i] = currentAxisValue;
                }

                currentTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator CalibrateZeros()
        {
            var samples = new float[axesCount];
            var currentTime = 0f;
            while (currentTime < 3f)
            {
                for (var i = 0; i < axesCount; i++)
                {
                    var currentAxisValue = _joystick.Axes[i].value;
                    zeroValues[i] += currentAxisValue;
                    samples[i]++;
                }

                currentTime += Time.deltaTime;
                yield return null;
            }

            for (var i = 0; i < axesCount; i++)
            {
                zeroValues[i] /= samples[i];
            }
        }

        private IEnumerator CheckAxis(Action<int> callback)
        {
            var axes = _joystick.Axes.ToList();
            Controller.Axis axis;

            while ((axis = axes.FirstOrDefault(a => a.valueDelta > 0.2f)) == null)
            {
                yield return null;
            }

            callback?.Invoke(axis.id);
        }

        private IEnumerator CheckButton (Action<int> callback)
        {
            var axes = _joystick.Buttons.ToList();
            Controller.Button button;

            while ((button = axes.FirstOrDefault(a => a.justPressed)) == null)
            {
                yield return null;
            }

            callback?.Invoke(button.id);
        }
        
        private IEnumerator WaitZeros()
        {
            var axes = _joystick.Axes.ToList();

            while (!axes.All(a => Mathf.Approximately(a.value, 0)))
            {
                yield return null;
            }
        }

        private void ApplyCalibration()
        {
            var axesCalibration = _joystick.calibrationMap.Axes;

            for (var i = 0; i < axesCount; i++)
            {
                var axis = axesCalibration[i];
                axis.calibratedZero = zeroValues[i];
                axis.calibratedMin = minValues[i];
                axis.calibratedMax = maxValues[i];
            }
        }

        private void BindAxes()
        {
            BindAxis(ElementAssignmentType.FullAxis, AxisRange.Full, _throttleAxisId, "Throttle");
            BindAxis(ElementAssignmentType.FullAxis, AxisRange.Full, _yawAxisId, "Yaw");
            BindAxis(ElementAssignmentType.FullAxis, AxisRange.Full, _pitchAxisId, "Pitch");
            BindAxis(ElementAssignmentType.FullAxis, AxisRange.Full, _rollAxisId, "Roll");
            BindAxis(ElementAssignmentType.Button, AxisRange.Positive, _cameraButtonId, "DroneCamera");
            BindAxis(ElementAssignmentType.Button, AxisRange.Positive, _modeButtonId, "DroneMode");
            BindAxis(ElementAssignmentType.Button, AxisRange.Positive, _restartButtonId, "DroneRestart");
        }

        private void BindAxis(ElementAssignmentType assignmentType, AxisRange axisRange, int axisId, string actionName)
        {
            var controllerMap = _player.controllers.maps.GetMap(ControllerType.Joystick, 0, "Default", "Default");
            if (controllerMap == null)
            {
                Debug.LogWarning("Controller map not found for player.");
                return;
            }

            var actionId = ReInput.mapping.GetActionId(actionName);
            
            var elementMaps = controllerMap.GetElementMapsWithAction(actionId);
            foreach (var elementMap in elementMaps)
                controllerMap.DeleteElementMap(elementMap.id);
            
            var assignment = new ElementAssignment(assignmentType, axisId, axisRange, KeyCode.None, ModifierKeyFlags.None, actionId, Pole.Positive, false, -1);
            controllerMap.CreateElementMap(assignment);
        }
    }
}