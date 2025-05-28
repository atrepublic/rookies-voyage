// Serializer.cs
// 이 스크립트는 게임 저장/로드 시스템에서 파일 직렬화 및 역직렬화를 처리하는 유틸리티 정적 클래스입니다.
// Application.persistentDataPath를 기본 저장 경로로 사용하며,
// BinaryFormatter를 이용한 이진 직렬화/역직렬화, 파일 존재 확인, 파일 삭제 기능을 제공합니다.

using System;
using System.IO; // 파일 I/O 작업을 위해 필요
using System.Runtime.Serialization.Formatters.Binary; // 이진 직렬화를 위해 필요
using UnityEngine; // Application.persistentDataPath를 위해 필요

namespace Watermelon
{
    // 파일 직렬화 및 역직렬화를 위한 정적 유틸리티 클래스입니다.
    public static class Serializer
    {
        // Application.persistentDataPath의 캐시된 경로입니다.
        private static string persistentDataPath;

        /// <summary>
        /// Serializer를 초기화하고 Application.persistentDataPath를 캐시하는 함수입니다.
        /// SaveController::Init 함수에서 호출되어야 합니다.
        /// </summary>
        public static void Init()
        {
            // Application.persistentDataPath를 가져와 캐시합니다.
            persistentDataPath = Application.persistentDataPath;
        }

        /// <summary>
        /// Persistent Data Path에 위치한 파일을 역직렬화하여 객체로 로드하는 제네릭 함수입니다.
        /// 파일이 존재하지 않으면 지정된 타입의 새로운 객체 인스턴스를 생성하여 반환합니다.
        /// </summary>
        /// <typeparam name="T">역직렬화할 객체의 타입 (기본 생성자 필수)</typeparam>
        /// <param name="fileName">역직렬화할 파일 이름</param>
        /// <param name="logIfFileNotExists">파일이 존재하지 않을 때 경고 메시지를 로그할지 여부 (기본값: false)</param>
        /// <returns>역직렬화된 객체 또는 새로운 객체 인스턴스</returns>
        public static T Deserialize<T>(string fileName, bool logIfFileNotExists = false) where T : new()
        {
            // Persistent Data Path와 파일 이름을 결합하여 절대 경로를 생성합니다.
            string absolutePath = Path.Combine(GetPersistentDataPath(), fileName);

            // 지정된 절대 경로에 파일이 존재하는지 확인합니다.
            if (FileExistsAtPath(absolutePath))
            {
                BinaryFormatter bf = new BinaryFormatter(); // 이진 직렬화/역직렬화 객체를 생성합니다.
                FileStream file = File.Open(absolutePath, FileMode.Open); // 파일을 읽기 모드로 엽니다.

                try
                {
                    // 파일 스트림에서 데이터를 역직렬화하여 지정된 타입 T의 객체로 변환합니다.
                    T deserializedObject = (T)bf.Deserialize(file);

                    return deserializedObject; // 역직렬화된 객체를 반환합니다.
                }
                catch (Exception ex) // 역직렬화 중 예외 발생 시
                {
                    Debug.LogError(ex.Message); // 오류 메시지를 로그합니다.
                    return new T(); // 새 객체 인스턴스를 생성하여 반환합니다.
                }
                finally // try/catch 블록 실행 후 항상 실행
                {
                    file.Close(); // 파일 스트림을 닫습니다.
                }
            }
            else // 파일이 존재하지 않으면
            {
                // 파일이 존재하지 않을 때 로그하도록 설정되어 있으면 경고 메시지를 출력합니다.
                if (logIfFileNotExists)
                {
                    Debug.LogWarning("File at path : \"" + absolutePath + "\" does not exist.");
                }
                return new T(); // 지정된 타입 T의 새로운 객체 인스턴스를 생성하여 반환합니다.
            }
        }

        /// <summary>
        /// 객체를 이진 형식으로 직렬화하여 Persistent Data Path에 파일로 저장하는 제네릭 함수입니다.
        /// </summary>
        /// <typeparam name="T">직렬화할 객체의 타입</typeparam>
        /// <param name="objectToSerialize">직렬화할 객체 참조</param>
        /// <param name="fileName">저장할 파일 이름</param>
        public static void Serialize<T>(T objectToSerialize, string fileName)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter(); // 이진 직렬화/역직렬화 객체를 생성합니다.
                // Persistent Data Path와 파일 이름을 결합한 경로에 파일을 생성 모드로 엽니다. using 블록으로 파일 스트림이 자동으로 닫히도록 합니다.
                using (FileStream file = File.Open(Path.Combine(GetPersistentDataPath(), fileName), FileMode.Create))
                {
                    bf.Serialize(file, objectToSerialize); // 객체를 이진 형식으로 직렬화하여 파일 스트림에 씁니다.
                }
            }
            catch // 직렬화 중 예외 발생 시
            {
                // 파일 직렬화에 실패했다는 것을 나타냅니다. (오류 로그는 생략)
            }
        }

        /// <summary>
        /// Persistent Data Path에 지정된 이름의 파일이 존재하는지 확인하는 함수입니다.
        /// </summary>
        /// <param name="fileName">확인할 파일 이름</param>
        /// <returns>파일이 존재하면 true, 그렇지 않으면 false</returns>
        public static bool FileExistsAtPDP(string fileName)
        {
            // Persistent Data Path와 파일 이름을 결합한 경로에 파일이 존재하는지 확인하여 반환합니다.
            return File.Exists(Path.Combine(GetPersistentDataPath(), fileName));
        }

        /// <summary>
        /// 지정된 절대 경로에 파일이 존재하는지 확인하는 함수입니다.
        /// </summary>
        /// <param name="absolutePath">확인할 파일의 절대 경로 (파일 이름 및 확장자 포함)</param>
        /// <returns>파일이 존재하면 true, 그렇지 않으면 false</returns>
        public static bool FileExistsAtPath(string absolutePath)
        {
            // 지정된 절대 경로에 파일이 존재하는지 확인하여 반환합니다.
            return File.Exists(absolutePath);
        }

        /// <summary>
        /// 지정된 디렉토리 경로에 지정된 이름의 파일이 존재하는지 확인하는 함수입니다.
        /// </summary>
        /// <param name="directoryPath">확인할 디렉토리의 전체 경로 (디렉토리 이름으로 끝나야 함, '/' 없음)</param>
        /// <param name="fileName">확인할 파일 이름</param>
        /// <returns>파일이 존재하면 true, 그렇지 않으면 false</returns>
        public static bool FileExistsAtPath(string directoryPath, string fileName)
        {
            // 디렉토리 경로와 파일 이름을 결합한 경로에 파일이 존재하는지 확인하여 반환합니다.
            return File.Exists(Path.Combine(directoryPath, fileName));
        }

        /// <summary>
        /// Persistent Data Path에 지정된 이름의 파일을 삭제하는 함수입니다.
        /// </summary>
        /// <param name="fileName">삭제할 파일 이름</param>
        public static void DeleteFileAtPDP(string fileName)
        {
            // Persistent Data Path와 파일 이름을 결합한 경로의 파일을 삭제합니다.
            File.Delete(Path.Combine(GetPersistentDataPath(), fileName));
        }

        /// <summary>
        /// 지정된 절대 경로의 파일을 삭제하는 함수입니다.
        /// </summary>
        /// <param name="absolutePath">삭제할 파일의 절대 경로 (파일 이름 및 확장자 포함)</param>
        public static void DeleteFileAtPath(string absolutePath)
        {
            // 지정된 절대 경로의 파일을 삭제합니다.
            File.Delete(absolutePath);
        }

        /// <summary>
        /// 지정된 디렉토리 경로에 지정된 이름의 파일을 삭제하는 함수입니다.
        /// </summary>
        /// <param name="fileName">삭제할 파일 이름</param>
        /// <param name="directoryPath">파일이 있는 디렉토리의 전체 경로 (디렉토리 이름으로 끝나야 함, '/' 없음)</param>
        public static void DeleteFileAtPath(string directoryPath, string fileName)
        {
            // 디렉토리 경로와 파일 이름을 결합한 경로의 파일을 삭제합니다.
            File.Delete(Path.Combine(directoryPath, fileName));
        }

        /// <summary>
        /// Application.persistentDataPath를 가져와 반환하는 함수입니다.
        /// 경로가 아직 캐시되지 않았으면 캐시합니다.
        /// </summary>
        /// <returns>Application.persistentDataPath 문자열</returns>
        private static string GetPersistentDataPath()
        {
            // persistentDataPath가 null이거나 비어 있으면
            if (string.IsNullOrEmpty(persistentDataPath))
            {
                // Application.persistentDataPath를 가져와 캐시합니다.
                persistentDataPath = Application.persistentDataPath;

                return persistentDataPath; // 가져온 경로를 반환합니다.
            }

            return persistentDataPath; // 캐시된 경로를 반환합니다.
        }
    }
}