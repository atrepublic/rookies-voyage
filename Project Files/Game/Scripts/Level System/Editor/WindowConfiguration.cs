// 이 스크립트는 Unity 에디터 창의 설정을 정의하는 데이터 클래스입니다.
// 창 제목, 스크립트 리로드 시 창 유지 여부, 창 및 내용의 최소/최대 크기 제한 등을 설정할 수 있습니다.
// Builder 패턴을 사용하여 설정 객체를 쉽게 생성하고 구성할 수 있습니다.

#pragma warning disable 649
using UnityEngine;
using System;

namespace Watermelon
{
    [System.Serializable] // 어셈블리 리로드 시 데이터를 유지하기 위해 필요합니다.
    public sealed class WindowConfiguration
    {
        // 에디터 창 제목
        private string windowTitle;
        // 스크립트 리로드 시 창을 열린 상태로 유지할지 여부
        private bool keepWindowOpenOnScriptReload;
        // 창 최소 크기 제한 여부
        private bool restrictWindowMinSize;
        // 창 최소 크기 값
        private Vector2 windowMinSize;
        // 창 최대 크기 제한 여부
        private bool restrictWindowMaxSize;
        // 창 최대 크기 값
        private Vector2 windowMaxSize;
        // 내용 최대 크기 제한 여부
        private bool restrictContentMaxSize;
        // 내용 최대 크기 값
        private Vector2 contentMaxSize;
        // 내용 높이 제한 여부
        private bool restictContentHeight;

        // 스크립트 리로드 시 창 유지 여부를 가져옵니다.
        [Tooltip("스크립트 리로드 시 에디터 창을 열린 상태로 유지할지 여부입니다.")]
        public bool KeepWindowOpenOnScriptReload { get => keepWindowOpenOnScriptReload; }
        // 창 최소 크기 제한 여부를 가져옵니다.
        [Tooltip("에디터 창의 최소 크기를 제한할지 여부입니다.")]
        public bool RestrictWindowMinSize { get => restrictWindowMinSize; }
        // 창 최소 크기 값을 가져오거나 설정합니다.
        [Tooltip("에디터 창의 최소 크기 값입니다.")]
        public Vector2 WindowMinSize { get => windowMinSize; set => windowMinSize = value; }
        // 창 최대 크기 제한 여부를 가져옵니다.
        [Tooltip("에디터 창의 최대 크기를 제한할지 여부입니다.")]
        public bool RestrictWindowMaxSize { get => restrictWindowMaxSize; }
        // 창 최대 크기 값을 가져오거나 설정합니다.
        [Tooltip("에디터 창의 최대 크기 값입니다.")]
        public Vector2 WindowMaxSize { get => windowMaxSize; set => windowMaxSize = value; }
        // 에디터 창 제목을 가져옵니다.
        [Tooltip("에디터 창의 제목입니다.")]
        public string WindowTitle => windowTitle;
        // 내용 최대 크기 제한 여부를 가져옵니다.
        [Tooltip("에디터 창 내용 영역의 최대 크기를 제한할지 여부입니다.")]
        public bool RestrictContentMaxSize => restrictContentMaxSize;
        // 내용 최대 크기 값을 가져옵니다.
        [Tooltip("에디터 창 내용 영역의 최대 크기 값입니다.")]
        public Vector2 ContentMaxSize => contentMaxSize;
        // 내용 높이 제한 여부를 가져옵니다.
        [Tooltip("에디터 창 내용 영역의 높이를 제한할지 여부입니다.")]
        public bool RestictContentHeight => restictContentHeight;


        // WindowConfiguration 클래스의 private 생성자
        // Builder 패턴을 사용하도록 강제합니다.
        private WindowConfiguration()
        {
            // 기본값 설정
            this.windowTitle = LevelEditorBase.DEFAULT_LEVEL_EDITOR_TITLE;
            this.windowMinSize = Vector2.one * LevelEditorBase.DEFAULT_WINDOW_MIN_SIZE;
        }

        // WindowConfiguration 객체를 단계별로 구성하기 위한 Builder 클래스
        public sealed class Builder
        {
            private WindowConfiguration editorConfiguration;

            // Builder 클래스의 생성자
            public Builder()
            {
                editorConfiguration = new WindowConfiguration(); // 새로운 WindowConfiguration 인스턴스 생성
            }

            // 에디터 창 제목을 설정합니다.
            // <param name="windowTitle">설정할 창 제목입니다.</param>
            // <returns>체이닝을 위한 Builder 인스턴스입니다.</returns>
            public Builder SetWindowTitle(string windowTitle)
            {
                editorConfiguration.windowTitle = windowTitle;
                return this;
            }

            // 스크립트 리로드 시 창 유지 여부를 설정합니다.
            // <param name="keepWindowOpenOnScriptReload">유지할지 여부입니다.</param>
            // <returns>체이닝을 위한 Builder 인스턴스입니다.</returns>
            public Builder KeepWindowOpenOnScriptReload(bool keepWindowOpenOnScriptReload)
            {
                editorConfiguration.keepWindowOpenOnScriptReload = keepWindowOpenOnScriptReload;
                return this;
            }

            // 창 최소 크기를 설정하고 제한을 활성화합니다.
            // <param name="windowMinSize">설정할 창 최소 크기입니다.</param>
            // <returns>체이닝을 위한 Builder 인스턴스입니다.</returns>
            public Builder SetWindowMinSize(Vector2 windowMinSize)
            {
                editorConfiguration.windowMinSize = windowMinSize;
                editorConfiguration.restrictWindowMinSize = true; // 최소 크기 제한 활성화
                return this;
            }

            // 창 최대 크기를 설정하고 제한을 활성화합니다.
            // <param name="windowMaxSize">설정할 창 최대 크기입니다.</param>
            // <returns>체이닝을 위한 Builder 인스턴스입니다.</returns>
            public Builder SetWindowMaxSize(Vector2 windowMaxSize)
            {
                editorConfiguration.windowMaxSize = windowMaxSize;
                editorConfiguration.restrictWindowMaxSize = true; // 최대 크기 제한 활성화
                return this;
            }

            // 내용 최대 크기를 설정하고 제한을 활성화합니다.
            // <param name="contentMaxSize">설정할 내용 최대 크기입니다.</param>
            // <returns>체이닝을 위한 Builder 인스턴스입니다.</returns>
            public Builder SetContentMaxSize(Vector2 contentMaxSize)
            {
                editorConfiguration.contentMaxSize = contentMaxSize;
                editorConfiguration.restrictContentMaxSize = true; // 내용 최대 크기 제한 활성화
                return this;
            }

            // 내용 높이 제한 여부를 설정합니다.
            // <param name="enable">제한 활성화 여부입니다.</param>
            // <returns>체이닝을 위한 Builder 인스턴스입니다.</returns>
            public Builder RestrictContentHeight(bool enable)
            {
                editorConfiguration.restictContentHeight = enable; // 내용 높이 제한 설정
                return this;
            }

            // 구성된 WindowConfiguration 객체를 빌드하여 반환합니다.
            // <returns>구성된 WindowConfiguration 인스턴스입니다.</returns>
            public WindowConfiguration Build()
            {
                return editorConfiguration;
            }
        }
    }
}