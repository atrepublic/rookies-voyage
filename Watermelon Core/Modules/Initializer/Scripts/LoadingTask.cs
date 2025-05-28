// LoadingTask.cs
// 이 스크립트는 게임 초기화 또는 로딩 과정에서 수행될 단일 비동기 또는 동기 작업을 정의하는 추상 클래스입니다.
// 각 작업은 활성화, 완료 상태, 완료 상태(성공/실패/건너뛰기)를 가집니다.
// 이 클래스를 상속받아 구체적인 로딩 작업을 구현합니다.

using System;

namespace Watermelon
{
    // 게임 로딩 및 초기화 과정에서 실행될 단일 작업을 나타내는 추상 클래스입니다.
    public abstract class LoadingTask
    {
        // 이 로딩 작업이 활성화되었는지 여부를 나타냅니다.
        public bool IsActive { get; private set; }
        // 이 로딩 작업이 완료되었는지 여부를 나타냅니다.
        public bool IsFinished { get; private set; }

        // 이 로딩 작업의 완료 상태 (Skipped, Completed, Failed)를 나타냅니다.
        public CompleteStatus Status { get; private set; }

        // 로딩 작업이 완료될 때 발생하며 작업 완료 상태를 전달하는 이벤트입니다.
        public event Action<CompleteStatus> OnTaskCompleted;

        /// <summary>
        /// LoadingTask 클래스의 생성자입니다.
        /// 작업의 활성화 및 완료 상태를 기본값(false)으로 초기화합니다.
        /// </summary>
        public LoadingTask()
        {
            IsActive = false; // 작업 활성화 상태를 false로 초기화합니다.
            IsFinished = false; // 작업 완료 상태를 false로 초기화합니다.
        }

        /// <summary>
        /// 로딩 작업을 완료 상태로 표시하고 관련 이벤트 핸들러를 호출하는 함수입니다.
        /// 이미 완료된 작업에 대해 다시 호출해도 아무런 효과가 없습니다.
        /// </summary>
        /// <param name="status">작업의 완료 상태 (Skipped, Completed, Failed)</param>
        public void CompleteTask(CompleteStatus status)
        {
            // 작업이 이미 완료되었으면 함수를 종료합니다.
            if (IsFinished) return;

            Status = status; // 작업 완료 상태를 설정합니다.
            IsFinished = true; // 작업 완료 플래그를 true로 설정합니다.

            // 작업 완료 이벤트 핸들러를 호출하고 완료 상태를 전달합니다.
            OnTaskCompleted?.Invoke(status);
        }

        /// <summary>
        /// 로딩 작업을 활성화하고 추상 메서드 OnTaskActivated()를 호출하는 함수입니다.
        /// OnTaskActivated() 실행 중 예외 발생 시 작업을 실패 상태로 완료합니다.
        /// </summary>
        public void Activate()
        {
            IsActive = true; // 작업 활성화 플래그를 true로 설정합니다.

            try
            {
                OnTaskActivated(); // 추상 메서드를 호출하여 실제 로딩 작업을 시작합니다.
            }
            catch // OnTaskActivated() 실행 중 예외 발생 시
            {
                CompleteTask(CompleteStatus.Failed); // 작업을 실패 상태로 완료합니다.
            }
        }

        /// <summary>
        /// 이 로딩 작업이 실제로 수행될 로직을 정의하는 추상 메서드입니다.
        /// 이 메서드는 작업이 활성화될 때 한 번 호출됩니다.
        /// 파생 클래스에서 반드시 구현해야 합니다.
        /// </summary>
        public abstract void OnTaskActivated();

        // 로딩 작업의 가능한 완료 상태를 정의하는 열거형입니다.
        public enum CompleteStatus {
            Skipped,   // 작업이 건너뛰어졌습니다.
            Completed, // 작업이 성공적으로 완료되었습니다.
            Failed     // 작업이 실패했습니다.
        }
    }
}