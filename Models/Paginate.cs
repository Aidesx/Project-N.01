namespace Project_385.Models
{
    public class Paginate
    {
        public int TotalItem { get; private set; }
        public int TotalPage { get; private set; }
        public int PageSize { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageStart { get; private set; }
        public int PageEnd { get; private set; }

        public Paginate()
        {

        }
        public Paginate(int totalItem,int page ,int pageSize = 10)
        {
            int totalPage= (int)Math.Ceiling((decimal)totalItem / (decimal)pageSize);

            int currentPage = page;
            int startPage = currentPage - 5;
            int endPage = currentPage + 4;

            if(startPage <= 0 )
            {
                endPage = endPage-(startPage-1);
                startPage = 1;
            }
            if(endPage> totalPage)
            {
                endPage = totalPage;
                if (endPage > 10)
                {
                    startPage = endPage-9;
                }
            }

            TotalItem = totalItem;
            CurrentPage = currentPage;
            PageSize=pageSize;
            TotalPage = totalPage;
            PageStart=startPage;
            PageEnd=endPage;

        }
    }
}
