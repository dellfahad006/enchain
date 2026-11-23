using System;
using System.Collections;
using UnityEngine;

public class CursorHideShow : MonoBehaviour
{
        public GameObject PauseGO;

        [NonSerialized]
        public bool paused;

        private bool isLocked;

        private bool allowRelockOnClick = true;

        public GameObject[] crosshairObjs;

        private void Start()
        {
                setCursorLock(true);
        }

        private void setCursorLock(bool isLocked)
        {
                this.isLocked = isLocked;
                if (isLocked)
                {
                        Cursor.lockState = CursorLockMode.Locked;
                        StartCoroutine(delayLock());
                }
                else
                {
                        Cursor.lockState = CursorLockMode.None;
                }
                Cursor.visible = !isLocked;
        }

        private void pauseGame(bool pause)
        {
                float timeScale = 1f;
                if (Time.timeScale != 0f)
                {
                        timeScale = Time.timeScale;
                }
                if (pause)
                {
                        timeScale = 0f;
                }
                Time.timeScale = timeScale;
                PauseGO.SetActive(pause);
                GameManager.instance.GiveTip();
                GameObject[] array = crosshairObjs;
                foreach (GameObject gameObject in array)
                {
                        gameObject.SetActive(!pause && DevSettings.showCrosshair);
                }
                paused = pause;
        }

        public void ApplyCrosshair()
        {
                if (crosshairObjs == null)
                {
                        return;
                }
                foreach (GameObject gameObject in crosshairObjs)
                {
                        if (gameObject != null && gameObject.activeSelf != (!paused && DevSettings.showCrosshair))
                        {
                                gameObject.SetActive(!paused && DevSettings.showCrosshair);
                        }
                }
        }

        private float devPrevScale = 1f;

        private bool devPrevLocked = true;

        // pause used by the dev menu: no pause screen, just frozen time and a free cursor
        public void DevPause(bool on)
        {
                if (on)
                {
                        devPrevLocked = isLocked;
                        devPrevScale = Time.timeScale > 0f ? Time.timeScale : 1f;
                        Time.timeScale = 0f;
                        paused = true;
                        setCursorLock(false);
                }
                else
                {
                        Time.timeScale = devPrevScale;
                        paused = false;
                        allowRelockOnClick = true;
                        setCursorLock(devPrevLocked);
                }
        }

        public void Unpause(bool lockCursor)
        {
                allowRelockOnClick = lockCursor;
                setCursorLock(lockCursor);
                pauseGame(false);
        }

        public void DOpause(bool a)
        {
                setCursorLock(!a);
                pauseGame(a);
        }

        private void Update()
        {
                if (!paused && allowRelockOnClick && Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
                {
                        setCursorLock(true);
                }
                if (!DevSettings.menuOpen && Time.frameCount != DevSettings.lastCloseFrame && (Input.GetKeyUp(KeyCode.Escape) || (Input.GetKeyDown(KeyCode.P) && !paused)))
                {
                        allowRelockOnClick = true;
                        setCursorLock(!isLocked);
                        pauseGame(!paused);
                }
        }

        private IEnumerator delayLock()
        {
                yield return 0;
                Cursor.lockState = CursorLockMode.Locked;
        }
}
