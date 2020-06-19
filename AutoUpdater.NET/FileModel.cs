namespace QI4A.ZIP
{
    /// <summary>
    /// 업데이트 파일 비교 모델
    /// </summary>
    public class FileModel
    {
        /// <summary>
        /// 파일 이름
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 파일 수정 날짜
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 파일 크기
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// 업데이트 파일의 고유값
        /// </summary>
        public string Hash { get; set; }
    }
}
