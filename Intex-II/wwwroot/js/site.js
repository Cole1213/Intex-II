// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
if (document.querySelectorAll(".carousel").length > 0) {
    let carousels = document.querySelectorAll(".carousel");
    carousels.forEach(carousel => newCarousel(carousel));

    function newCarousel(carousel) {
        let carouselChildren = document.querySelector(
            `.carousel[data-carousel="${carousel.dataset.carousel}"]`
        ).children;
        let speed = carousel.dataset.speed;
        let carouselContent = document.querySelectorAll(`.carousel-content`)[
        carousel.dataset.carousel - 1
            ];
        const carouselLength = carouselContent.children.length;
        let width = window.innerWidth / 2;
        let count = width;
        let counterIncrement = width;
        let int = setInterval(timer, speed);

        // initial transform
        carouselContent.style.transform = `translateX(-${width}px)`;

        function timer() {
            if (count >= (counterIncrement - 2) * (carouselLength - 2)) {
                count = 0;
                carouselContent.style.transform = `translateX(-${count}px)`;
            }
            count = count + counterIncrement;
            carouselContent.style.transform = `translateX(-${count}px)`;
        }

        function carouselClick() {
            // left click
            carouselChildren[0].addEventListener("click", function() {
                count = count - width;
                carouselContent.style.transform = `translateX(-${count - 100}px)`;
                if (count < counterIncrement) {
                    count = counterIncrement * (carouselLength - 2);
                    carouselContent.style.transform = `translateX(-${count - 100}px)`;
                }
            });
            // right click
            carouselChildren[1].addEventListener("click", function() {
                count = count + width;
                carouselContent.style.transform = `translateX(-${count + 100}px)`;
                if (count >= counterIncrement * (carouselLength - 1)) {
                    count = counterIncrement;
                    carouselContent.style.transform = `translateX(-${count + 100}px)`;
                }
            });
        }

        function carouselHoverEffect() {
            // left hover effect events
            carouselChildren[0].addEventListener("mouseenter", function() {
                carouselContent.style.transform = `translateX(-${count - 100}px)`;
                clearInterval(int);
            });
            carouselChildren[0].addEventListener("mouseleave", function() {
                carouselContent.style.transform = `translateX(-${count}px)`;
                int = setInterval(timer, speed);
            });

            // right hover effect events
            carouselChildren[1].addEventListener("mouseenter", function() {
                carouselContent.style.transform = `translateX(-${count + 100}px)`;
                clearInterval(int);
            });
            carouselChildren[1].addEventListener("mouseleave", function() {
                carouselContent.style.transform = `translateX(-${count}px)`;
                int = setInterval(timer, speed);
            });
        }

        carouselHoverEffect();
        carouselClick();
    }
}
